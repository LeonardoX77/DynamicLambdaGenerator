using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Common.Core.Generic.DynamicQueryFilter.Interfaces;

namespace Common.Core.Generic.DynamicQueryFilter.DynamicExpressions
{
    /// <summary>
    /// Filter by date criteria in a generic entity.
    /// </summary>
    public class DateRangeExpression<T, TQueryFilter> : DynamicExpression<T, TQueryFilter>
    where T : class, new()
    where TQueryFilter : class, IDynamicQueryFilter, new()
    {
        private readonly DateTime? _from;
        private readonly DateTime? _to;
        private readonly string _propertyName; // Name of the property to compare

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="propertyName">Name of the DateTime property in the entity.</param>
        /// <param name="from">From date.</param>
        /// <param name="to">To date.</param>
        public DateRangeExpression(string propertyName, DateTime? from = null, DateTime? to = null)
        {
            _propertyName = propertyName;
            _from = from;
            _to = to;
            _predicate = SetPredicate();
        }

        /// <inheritdoc/>
        protected override Expression<Func<T, bool>> SetPredicate()
        {
            Expression<Func<T, bool>> fromExpression = _from.HasValue ?
                (entity => EF.Property<DateTime>(entity, _propertyName) >= _from.Value) :
                _ => true;
            Expression<Func<T, bool>> toExpression = _to.HasValue ?
                (entity => EF.Property<DateTime>(entity, _propertyName) <= _to.Value) :
                _ => true;

            // Combine the two expressions with a logical AND
            return entity => fromExpression.Compile()(entity) && toExpression.Compile()(entity);
        }
    }
}
