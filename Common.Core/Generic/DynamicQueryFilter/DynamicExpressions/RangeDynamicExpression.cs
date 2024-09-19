using Common.Core.Generic.DynamicQueryFilter.Interfaces;
using System.Linq.Expressions;

namespace Common.Core.Generic.DynamicQueryFilter.DynamicExpressions
{
    /// <summary>
    /// Filter for a range of values applicable to any entity.
    /// </summary>
    /// <typeparam name="T">Entity</typeparam>
    /// <typeparam name="TQueryFilter">DTO Entity</typeparam>
    public class RangeDynamicExpression<T, TQueryFilter> : DynamicExpression<T, TQueryFilter>
        where T : class, new()
        where TQueryFilter : class, IDynamicQueryFilter, new()
    {
        private readonly object _greaterThanOrEqualValue;
        private readonly object _lessThanOrEqualValue;
        private readonly object _equalValue;
        private readonly object _greaterThanValue;
        private readonly object _lessThanValue;
        private readonly string _propertyName;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="propertyName">The name of the property to filter.</param>
        /// <param name="greaterThanOrEqualValue">Minimum value for the range filter.</param>
        /// <param name="lessThanOrEqualValue">Maximum value for the range filter.</param>
        /// <param name="equalValue">Specific value for equality filter.</param>
        /// <param name="greaterThanValue">Minimum exclusive value for the range filter.</param>
        /// <param name="lessThanValue">Maximum exclusive value for the range filter.</param>
        public RangeDynamicExpression(
            string propertyName, 
            object greaterThanOrEqualValue = null, 
            object lessThanOrEqualValue = null, 
            object equalValue = null,
            object greaterThanValue = null,
            object lessThanValue = null)
        {
            _propertyName = propertyName;
            _greaterThanOrEqualValue = greaterThanOrEqualValue;
            _lessThanOrEqualValue = lessThanOrEqualValue;
            _equalValue = equalValue;
            _greaterThanValue = greaterThanValue;
            _lessThanValue = lessThanValue;
            _predicate = SetPredicate();
        }

        /// <summary>
        /// Sets the predicate for the range filter.
        /// </summary>
        /// <returns>Expression representing the predicate.</returns>
        protected override Expression<Func<T, bool>> SetPredicate()
        {
            var parameter = Expression.Parameter(typeof(T), "entity");
            var propertyExpression = Expression.PropertyOrField(parameter, _propertyName);
            Expression finalExpression = Expression.Constant(true, typeof(bool));

            // Apply minimum value comparison if specified
            finalExpression = GetGreaterThanOrEqualExpression(propertyExpression, finalExpression);

            // Apply maximum value comparison if specified
            finalExpression = GetLessThanOrEqualExpression(propertyExpression, finalExpression);

            // Apply equality comparison if specified
            finalExpression = GetEqualExpression(propertyExpression, finalExpression);

            // Apply exclusive minimum value comparison if specified
            finalExpression = GetGreaterThanExpression(propertyExpression, finalExpression);

            // Apply exclusive maximum value comparison if specified
            finalExpression = GetLessThanExpression(propertyExpression, finalExpression);

            return Expression.Lambda<Func<T, bool>>(finalExpression, parameter);
        }

        /// <summary>
        /// Creates an equality comparison expression.
        /// </summary>
        /// <param name="propertyExpression">The property expression.</param>
        /// <param name="finalExpression">The final expression to combine with.</param>
        /// <returns>Combined expression with equality comparison.</returns>
        private Expression GetEqualExpression(MemberExpression propertyExpression, Expression finalExpression)
        {
            if (_equalValue != null)
            {
                Expression equalExpression = Expression.Equal(
                    Expression.Convert(propertyExpression, _equalValue.GetType()),
                    Expression.Constant(_equalValue, _equalValue.GetType())
                );
                finalExpression = Expression.AndAlso(finalExpression, equalExpression);
            }

            return finalExpression;
        }

        /// <summary>
        /// Creates a maximum value comparison expression.
        /// </summary>
        /// <param name="propertyExpression">The property expression.</param>
        /// <param name="finalExpression">The final expression to combine with.</param>
        /// <returns>Combined expression with maximum value comparison.</returns>
        private Expression GetLessThanOrEqualExpression(MemberExpression propertyExpression, Expression finalExpression)
        {
            if (_lessThanOrEqualValue != null)
            {
                Expression maxExpression = Expression.LessThanOrEqual(
                    Expression.Convert(propertyExpression, _lessThanOrEqualValue.GetType()),
                    Expression.Constant(_lessThanOrEqualValue, _lessThanOrEqualValue.GetType())
                );
                finalExpression = Expression.AndAlso(finalExpression, maxExpression);
            }

            return finalExpression;
        }

        /// <summary>
        /// Creates a minimum value comparison expression.
        /// </summary>
        /// <param name="propertyExpression">The property expression.</param>
        /// <param name="finalExpression">The final expression to combine with.</param>
        /// <returns>Combined expression with minimum value comparison.</returns>
        private Expression GetGreaterThanOrEqualExpression(MemberExpression propertyExpression, Expression finalExpression)
        {
            if (_greaterThanOrEqualValue != null)
            {
                Expression minExpression = Expression.GreaterThanOrEqual(
                    Expression.Convert(propertyExpression, _greaterThanOrEqualValue.GetType()),
                    Expression.Constant(_greaterThanOrEqualValue, _greaterThanOrEqualValue.GetType())
                );
                finalExpression = Expression.AndAlso(finalExpression, minExpression);
            }

            return finalExpression;
        }

        /// <summary>
        /// Creates an exclusive minimum value comparison expression.
        /// </summary>
        /// <param name="propertyExpression">The property expression.</param>
        /// <param name="finalExpression">The final expression to combine with.</param>
        /// <returns>Combined expression with exclusive minimum value comparison.</returns>
        private Expression GetGreaterThanExpression(MemberExpression propertyExpression, Expression finalExpression)
        {
            if (_greaterThanValue != null)
            {
                Expression minExpression = Expression.GreaterThan(
                    Expression.Convert(propertyExpression, _greaterThanValue.GetType()),
                    Expression.Constant(_greaterThanValue, _greaterThanValue.GetType())
                );
                finalExpression = Expression.AndAlso(finalExpression, minExpression);
            }

            return finalExpression;
        }

        /// <summary>
        /// Creates an exclusive maximum value comparison expression.
        /// </summary>
        /// <param name="propertyExpression">The property expression.</param>
        /// <param name="finalExpression">The final expression to combine with.</param>
        /// <returns>Combined expression with exclusive maximum value comparison.</returns>
        private Expression GetLessThanExpression(MemberExpression propertyExpression, Expression finalExpression)
        {
            if (_lessThanValue != null)
            {
                Expression maxExpression = Expression.LessThan(
                    Expression.Convert(propertyExpression, _lessThanValue.GetType()),
                    Expression.Constant(_lessThanValue, _lessThanValue.GetType())
                );
                finalExpression = Expression.AndAlso(finalExpression, maxExpression);
            }

            return finalExpression;
        }
    }
}
