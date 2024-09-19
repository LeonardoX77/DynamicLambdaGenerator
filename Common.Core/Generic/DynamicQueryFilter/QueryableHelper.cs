using Common.Core.Data.Interfaces;
using System.Linq.Expressions;

namespace Common.Core.Generic.QueryLanguage
{
    /// <summary>
    /// Helper class to apply sorting to IQueryable objects.
    /// </summary>
    public static class QueryableHelper<T>
    {
        /// <summary>
        /// Class representing a sorting descriptor with a field name and sorting order.
        /// </summary>
        public class SortDescriptor
        {
            /// <summary>
            /// Name of the field to sort by.
            /// </summary>
            public string FieldName { get; set; }
            /// <summary>
            /// Indicates whether the sorting is ascending. Default is true (ascending).
            /// </summary>
            public bool IsAscending { get; set; } = true; // Default to ascending
        }

        /// <summary>
        /// Applies sorting to the provided query based on the sorting fields in the pagination object.
        /// </summary>
        /// <param name="query">The query to apply sorting to.</param>
        /// <param name="pagination">Pagination object containing sorting fields.</param>
        /// <returns>The sorted query.</returns>
        public static IQueryable<T> ApplySorting(IQueryable<T> query, IPagination pagination)
        {
            if (!string.IsNullOrEmpty(pagination.SortingFields))
            {
                var sortDescriptors = ParseSortingVariable(pagination.SortingFields);
                var first = true;

                foreach (var descriptor in sortDescriptors)
                {
                    Expression<Func<T, object>> expression = GetOrderByExpression(descriptor.FieldName);

                    if (first)
                    {
                        query = descriptor.IsAscending
                            ? query.OrderBy(expression)
                            : query.OrderByDescending(expression);
                        first = false;
                    }
                    else
                    {
                        query = descriptor.IsAscending
                            ? ((IOrderedQueryable<T>)query).ThenBy(expression)
                            : ((IOrderedQueryable<T>)query).ThenByDescending(expression);
                    }
                }
            }

            return query;
        }

        /// <summary>
        /// Creates an expression for ordering by the specified property name.
        /// </summary>
        /// <param name="propertyName">The property name to order by.</param>
        /// <returns>An expression representing the order by operation.</returns>
        private static Expression<Func<T, object>> GetOrderByExpression(string propertyName)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            Expression property = parameter;
            foreach (var prop in propertyName.Split('.'))
            {
                property = Expression.PropertyOrField(property, prop);
            }

            // Ensure the return type of the expression is object to be used in generic OrderBy methods
            return Expression.Lambda<Func<T, object>>(Expression.Convert(property, typeof(object)), parameter);
        }

        /// <summary>
        /// Parses the sorting variable string into a list of SortDescriptor objects.
        /// </summary>
        /// <param name="sortingVariable">The sorting variable string.</param>
        /// <returns>A list of SortDescriptor objects.</returns>
        private static List<SortDescriptor> ParseSortingVariable(string sortingVariable)
        {
            var descriptors = new List<SortDescriptor>();

            var sortingParts = sortingVariable.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in sortingParts)
            {
                var spaceIndex = part.LastIndexOf(" ");
                if (spaceIndex != -1 && (part.EndsWith(" desc", StringComparison.OrdinalIgnoreCase) || part.EndsWith(" asc", StringComparison.OrdinalIgnoreCase)))
                {
                    var fieldName = part.Substring(0, spaceIndex);
                    var order = part.Substring(spaceIndex + 1).Trim();
                    descriptors.Add(new SortDescriptor
                    {
                        FieldName = fieldName,
                        IsAscending = order.Equals("asc", StringComparison.OrdinalIgnoreCase)
                    });
                }
                else
                {
                    descriptors.Add(new SortDescriptor { FieldName = part });
                }
            }

            return descriptors;
        }
    }
}
