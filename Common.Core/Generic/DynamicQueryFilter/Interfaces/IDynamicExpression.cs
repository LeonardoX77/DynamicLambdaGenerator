using System.Linq.Expressions;

namespace Common.Core.Generic.QueryLanguage.Interfaces
{
    /// <summary>
    /// Base interface for implementing the Specification/Criteria pattern.
    /// </summary>
    /// <typeparam name="T">Entity</typeparam>
    public interface IDynamicExpression<T> where T : class
    {
        /// <summary>
        /// Returns the predicate expression.
        /// </summary>
        /// <returns>Expression representing the predicate.</returns>
        Expression<Func<T, bool>> Predicate();
    }
}
