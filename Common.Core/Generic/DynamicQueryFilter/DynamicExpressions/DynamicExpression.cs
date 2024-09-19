using System.Linq.Expressions;
using System.Reflection;
using System.Collections;
using Common.Core.Generic.QueryLanguage.Interfaces;
using Common.Core.Generic.DynamicQueryFilter.Interfaces;

namespace Common.Core.Generic.DynamicQueryFilter.DynamicExpressions
{
    /// <summary>
    /// Base implementation of the Specification pattern.
    /// </summary>
    /// <typeparam name="T">Entity.</typeparam>
    /// <typeparam name="TQueryFilter">QueryFilter Entity.</typeparam>
    public class DynamicExpression<T, TQueryFilter> : IDynamicExpression<T>
    where T : class, new()
    where TQueryFilter : class, IDynamicQueryFilter, new()
    {
        /// <summary>
        /// Predicate expression.
        /// </summary>
        protected Expression<Func<T, bool>> _predicate;
        private readonly DynamicFiltersConfiguration _dynamicFiltersConfiguration;

        /// <summary>
        /// Returns the predicate.
        /// </summary>
        public Expression<Func<T, bool>> Predicate() => _predicate;

        /// <summary>
        /// Constructor.
        /// </summary>
        public DynamicExpression() { }
        public DynamicExpression(TQueryFilter filter, DynamicFiltersConfiguration config)
        {
            _dynamicFiltersConfiguration = config;
            _predicate = SetPredicate(filter);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="predicate">Predicate expression.</param>
        internal DynamicExpression(Expression<Func<T, bool>> predicate)
        {
            _predicate = predicate;
        }


        /// <summary>
        /// Sets the predicate to be used by Specification.
        /// </summary>
        /// <returns>Expression.</returns>
        protected virtual Expression<Func<T, bool>> SetPredicate() { return null; }
        protected Expression<Func<T, bool>> SetPredicate(TQueryFilter filter)
        {
            DynamicExpression<T, TQueryFilter> combinedSpec = new TrueDynamicExpression<T, TQueryFilter>();
            // Get only TDto properties
            var properties = filter.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                    .Where(p => p.DeclaringType == typeof(TQueryFilter));

            CheckNonNullableProperties(properties);

            foreach (var propertyInfo in properties)
            {
                object value = propertyInfo.GetValue(filter, null);
                if (value == null) continue;

                // Handle nested properties
                if (IsNestedProperty(propertyInfo))
                {
                    combinedSpec = HandleNestedProperties(combinedSpec, propertyInfo, value);
                }
                else
                {
                    combinedSpec &= CreatePropertySpecification(propertyInfo.Name, value);
                }
            }
            return combinedSpec.Predicate();
        }

        /// <summary>
        /// Check non nullable properties and raise an exception if any is present.
        /// Non nullable properties have default values which could be included in the dynamic query expression filter as condition for the returned query
        /// </summary>
        /// <param name="properties"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void CheckNonNullableProperties(IEnumerable<PropertyInfo> properties)
        {
            // Check for non-nullable properties
            var nonNullableProperties = properties
                .Where(p => !IsNullableProperty(p))
                .ToList();

            if (nonNullableProperties.Any())
            {
                var propertyNames = string.Join(", ", nonNullableProperties.Select(p => p.Name));
                throw new InvalidOperationException($"The following properties are not nullable: {propertyNames}. " +
                    "Non nullable properties have default values which could be included in the dynamic query expression filter as condition for the returned query. " +
                    "If you want to declare required properties please implement an IsRequired() validation in an AbstractValidator<TEntity> class");
            }
        }

        private bool IsNullableProperty(PropertyInfo propertyInfo)
        {
            if (!propertyInfo.PropertyType.IsValueType) return true; // Reference types are nullable
            if (Nullable.GetUnderlyingType(propertyInfo.PropertyType) != null) return true; // Nullable<T>
            return false; // Value types are not nullable
        }

        private DynamicExpression<T, TQueryFilter> HandleNestedProperties(DynamicExpression<T, TQueryFilter> combinedSpec, PropertyInfo propertyInfo, object value)
        {
            foreach (var nestedProperty in propertyInfo.PropertyType.GetProperties())
            {
                object nestedValue = nestedProperty.GetValue(value, null);
                if (nestedValue != null)
                {
                    string nestedPropertyPath = $"{propertyInfo.Name}.{nestedProperty.Name}";
                    combinedSpec &= CreatePropertySpecification(nestedPropertyPath, nestedValue);
                }
            }

            return combinedSpec;
        }

        /// <summary>
        /// And Specification.
        /// </summary>
        /// <param name="left">Specification.</param>
        /// <param name="right">Specification.</param>
        /// <returns>The Specification resulting from the And operation.</returns>
        public static DynamicExpression<T, TQueryFilter> operator &(DynamicExpression<T, TQueryFilter> left, DynamicExpression<T, TQueryFilter> right)
        {
            InvocationExpression rightInvoke = Expression.Invoke(right.Predicate(), left.Predicate().Parameters.Cast<Expression>());

            BinaryExpression newExpression = Expression.MakeBinary(ExpressionType.AndAlso, left.Predicate().Body, rightInvoke);

            return new DynamicExpression<T, TQueryFilter>(Expression.Lambda<Func<T, bool>>(newExpression, left.Predicate().Parameters));
        }

        /// <summary>
        /// Or Specification.
        /// </summary>
        /// <param name="left">Specification.</param>
        /// <param name="right">Specification.</param>
        /// <returns>The Specification resulting from the Or operation.</returns>
        public static DynamicExpression<T, TQueryFilter> operator |(DynamicExpression<T, TQueryFilter> left, DynamicExpression<T, TQueryFilter> right)
        {
            InvocationExpression rightInvoke = Expression.Invoke(right.Predicate(), left.Predicate().Parameters.Cast<Expression>());

            BinaryExpression newExpression = Expression.MakeBinary(ExpressionType.Or, left.Predicate().Body, rightInvoke);

            return new DynamicExpression<T, TQueryFilter>(Expression.Lambda<Func<T, bool>>(newExpression, left.Predicate().Parameters));
        }

        protected DynamicExpression<T, TQueryFilter> CreateSpecificationForProperty(string propertyPath, object value)
        {
            var parameter = Expression.Parameter(typeof(T), "entity");
            Expression propertyExpression = BuildPropertyExpression(parameter, propertyPath);

            return BuildSpecification(propertyExpression, value, parameter, propertyPath);
        }

        private string RemovePrefix(string propertyName)
        {
            string prefix = Array.Find(_dynamicFiltersConfiguration.PREFIXES, p => propertyName.StartsWith(p, StringComparison.OrdinalIgnoreCase));
            return prefix != null ? propertyName.Substring(prefix.Length) : propertyName;
        }

        private Expression BuildPropertyExpression(ParameterExpression parameterExpr, string propertyPath)
        {
            Expression propertyExpr = parameterExpr;
            foreach (var propertyName in propertyPath.Split('.'))
            {
                string actualPropertyName = RemovePrefix(propertyName);

                // Verify if the property or field actually exists
                var memberInfo = GetPropertyOrField(propertyExpr.Type, actualPropertyName);
                if (memberInfo == null)
                {
                    throw new ArgumentException($"'{actualPropertyName}' is not a member of type '{parameterExpr.Type}'", actualPropertyName);
                }

                propertyExpr = Expression.PropertyOrField(propertyExpr, actualPropertyName);
            }
            return propertyExpr;
        }

        private static MemberInfo GetPropertyOrField(Type type, string memberName)
        {
            // First check if it's a property
            var propertyInfo = type.GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance);
            if (propertyInfo != null) return propertyInfo;

            // Check if it's a field
            var fieldInfo = type.GetField(memberName, BindingFlags.Public | BindingFlags.Instance);
            if (fieldInfo != null) return fieldInfo;

            // Not found as either property or field
            return null;
        }

        private DynamicExpression<T, TQueryFilter> BuildSpecification(Expression propertyExpression, object value, ParameterExpression parameter, string propertyPath)
        {
            string[] pathSegments = propertyPath.Split('.');
            string propertyName = pathSegments[pathSegments.Length - 1];
            string propertyNameWithoutPrefix = RemovePrefix(propertyName);
            Type propertyType = value.GetType();

            // Handling List<>
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>)
                && propertyName.ToLower().StartsWith(_dynamicFiltersConfiguration.ListFieldName))
            {
                var list = ((IEnumerable)value).Cast<object>().ToList();
                if (list.Count > 0)
                {
                    return new ListDynamicExpression<T, TQueryFilter>(propertyNameWithoutPrefix, list);
                }
            }

            // Handling string properties
            if (propertyType == typeof(string))
            {
                if (propertyName.ToLower().StartsWith(_dynamicFiltersConfiguration.ContainsFieldName))
                {
                    var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                    MethodCallExpression containsExpression = Expression.Call(propertyExpression, containsMethod, Expression.Constant(value));

                    return new DynamicExpression<T, TQueryFilter>(Expression.Lambda<Func<T, bool>>(
                        containsExpression,
                        parameter));
                }
                else
                {
                    BinaryExpression expression = Expression.Equal(propertyExpression, Expression.Constant(value));

                    return new DynamicExpression<T, TQueryFilter>(Expression.Lambda<Func<T, bool>>(
                        expression,
                        parameter));
                }
            }

            // Handling ranges
            if (propertyType == typeof(int) || propertyType == typeof(double) || propertyType == typeof(float) || propertyType == typeof(decimal)
                || propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
            {
                return CreateDynamicExpressionRange(value, propertyName, propertyNameWithoutPrefix);
            }

            return new TrueDynamicExpression<T, TQueryFilter>();
        }

        private DynamicExpression<T, TQueryFilter> CreateDynamicExpressionRange(object value, string propertyName, string propertyNameWithoutPrefix)
        {
            if (propertyName.ToLower().StartsWith(_dynamicFiltersConfiguration.GreaterThanOrEqualFieldName.ToLower()))
            {
                return new RangeDynamicExpression<T, TQueryFilter>(propertyNameWithoutPrefix, greaterThanOrEqualValue: value);
            }
            else if (propertyName.ToLower().StartsWith(_dynamicFiltersConfiguration.GreaterThanFieldName.ToLower()))
            {
                return new RangeDynamicExpression<T, TQueryFilter>(propertyNameWithoutPrefix, greaterThanValue: value);
            }
            else if (propertyName.ToLower().StartsWith(_dynamicFiltersConfiguration.LessThanOrEqualFieldName.ToLower()))
            {
                return new RangeDynamicExpression<T, TQueryFilter>(propertyNameWithoutPrefix, lessThanOrEqualValue: value);
            }
            else if (propertyName.ToLower().StartsWith(_dynamicFiltersConfiguration.LessThanFieldName.ToLower()))
            {
                return new RangeDynamicExpression<T, TQueryFilter>(propertyNameWithoutPrefix, lessThanValue: value);
            }
            else
            {
                return new RangeDynamicExpression<T, TQueryFilter>(propertyName, equalValue: value);
            }
        }

        protected DynamicExpression<T, TQueryFilter> CreatePropertySpecification(string propertyPath, object value)
        {
            var propertyInfo = typeof(T).GetProperty(propertyPath.Split('.')[0]);

            if (IsNestedProperty(propertyInfo))
            {
                return HandleNestedProperty(propertyPath, value, propertyInfo);
            }

            return CreateSpecificationForProperty(propertyPath, value);
        }

        private DynamicExpression<T, TQueryFilter> HandleNestedProperty(string propertyPath, object value, PropertyInfo propertyInfo)
        {
            // Recursively handle nested properties only if they are user-defined types and not system types
            var nestedProperties = propertyInfo.PropertyType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            DynamicExpression<T, TQueryFilter> nestedSpec = new TrueDynamicExpression<T, TQueryFilter>();

            string[] pathSegments = propertyPath.Split('.');
            string nestedPropertyName = pathSegments[pathSegments.Length - 1];
            nestedPropertyName = RemovePrefix(nestedPropertyName);

            foreach (var nestedProperty in nestedProperties)
            {
                if (nestedProperty.Name == nestedPropertyName)
                {
                    var nestedPropertySpec = CreateSpecificationForProperty(propertyPath, value);
                    nestedSpec &= nestedPropertySpec;
                    break; // Desired property found
                }
            }

            return nestedSpec;
        }

        private static bool IsNestedProperty(PropertyInfo propertyInfo)
        {
            return propertyInfo != null && propertyInfo.PropertyType.IsClass && !propertyInfo.PropertyType.Namespace.StartsWith("System") && propertyInfo.PropertyType != typeof(string);
        }
    }
}
