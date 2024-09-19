using Common.Core.Generic.DynamicQueryFilter.Interfaces;
using System.Linq.Expressions;
using System.Reflection;

namespace Common.Core.Generic.DynamicQueryFilter.DynamicExpressions
{
    /// <summary>
    /// Filter by the criterion of a list of identifiers applicable to any entity.
    /// </summary>
    /// <typeparam name="T">Entity</typeparam>
    /// <typeparam name="TQueryFilter">DTO Entity</typeparam>
    public class ListDynamicExpression<T, TQueryFilter> : DynamicExpression<T, TQueryFilter>
    where T : class, new()
    where TQueryFilter : class, IDynamicQueryFilter, new()
    {
        private readonly List<object> _ids;
        private readonly string _propertyName;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="ids">List of identifiers.</param>
        public ListDynamicExpression(string propertyName, List<object> ids)
        {
            _ids = ids;
            _propertyName = propertyName;
            _predicate = SetPredicate();
        }

        /// <inheritdoc/>
        protected override Expression<Func<T, bool>> SetPredicate()
        {
            if (_ids == null) return x => true;

            DynamicExpression<T, TQueryFilter> specification = null;

            var parameter = Expression.Parameter(typeof(T), "entity");
            var propertyExpression = Expression.Property(parameter, _propertyName);

            if (_ids.Any())
            {
                _ids.ForEach(id =>
                {
                    var newSpecification = new DynamicExpression<T, TQueryFilter>(Expression.Lambda<Func<T, bool>>(Expression.Equal(propertyExpression, Expression.Constant(id)), parameter));

                    specification = specification == null ?
                        newSpecification :
                        specification | newSpecification;
                });
            } else
            {
                specification = new TrueDynamicExpression<T, TQueryFilter>();
            }

            return specification.Predicate();
        }
    }
}
