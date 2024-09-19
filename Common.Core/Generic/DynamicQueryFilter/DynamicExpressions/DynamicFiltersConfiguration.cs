namespace Common.Core.Generic.DynamicQueryFilter.DynamicExpressions
{
    /// <summary>
    /// This class defines the dynamic field prefix names for QueryFilters, such as those used in the ClientDynamicFieldsQueryFilter class.
    /// </summary>
    public class DynamicFiltersConfiguration
    {
        /// <summary>
        /// The name of the assembly containing the model definitions.
        /// </summary>
        public string AssemblyModelName { get; set; }

        /// <summary>
        /// Array of prefixes used in dynamic field filtering.
        /// For example, the ClientDynamicFieldsQueryFilter class may have properties prefixed with LessThanOrEqualFieldName, GreaterThanOrEqualFieldName, GreaterThanFieldName, LessThanFieldName, Contains, and List.
        /// </summary>
        public string[] PREFIXES { get => [LessThanOrEqualFieldName, GreaterThanOrEqualFieldName, GreaterThanFieldName, LessThanFieldName, ContainsFieldName, ListFieldName]; }

        /// <summary>
        /// Prefix for less than or equal value filters.
        /// </summary>
        public string LessThanOrEqualFieldName { get; set; }

        /// <summary>
        /// Prefix for greater than or equal value filters.
        /// </summary>
        public string GreaterThanOrEqualFieldName { get; set; }

        /// <summary>
        /// Prefix for greater than value filters.
        /// </summary>
        public string GreaterThanFieldName { get; set; }

        /// <summary>
        /// Prefix for less than value filters.
        /// </summary>
        public string LessThanFieldName { get; set; }

        /// <summary>
        /// Prefix for contains or substring filters.
        /// </summary>
        public string ContainsFieldName { get; set; }

        /// <summary>
        /// Prefix for list or collection filters.
        /// </summary>
        public string ListFieldName { get; set; }
    }
}
