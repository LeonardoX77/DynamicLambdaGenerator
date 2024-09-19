using Common.Core.Generic.DynamicQueryFilter.Interfaces;

namespace Common.Core.Generic.DynamicQueryFilter.DynamicExpressions
{
    public class TrueDynamicExpression<T, TQueryFilter> : DynamicExpression<T, TQueryFilter>
    where T : class, new()
    where TQueryFilter : class, IDynamicQueryFilter, new()
    {
        public TrueDynamicExpression() : base(x => true) { }
    }
}
