using Common.WebApi.Infrastructure.Models.Request;
using Common.Core.Generic.DynamicQueryFilter.Interfaces;
using System;
using System.Collections.Generic;

namespace Common.WebApi.Application.Models.Location
{
#nullable enable

    /// <summary>
    /// Filter for dynamic Fields
    /// </summary>
    public class LocationDynamicFieldsQueryFilter : BaseRequest, IDynamicQueryFilter
    {
        public List<int>? ListId { get; set; }

        /// <summary>
        /// Maximum ID for the filter (LessThanOrEqual).
        /// </summary>
        public int? LessThanOrEqualId { get; set; }

        /// <summary>
        /// Minimum ID for the filter (GreaterThanOrEqual).
        /// </summary>
        public int? GreaterThanOrEqualId { get; set; }

        /// <summary>
        /// Greater than ID for the filter (GreaterThan).
        /// </summary>
        public int? GreaterThanId { get; set; }

        /// <summary>
        /// Less than ID for the filter (LessThan).
        /// </summary>
        public int? LessThanId { get; set; }

        /// <summary>
        /// Contains filter for name.
        /// </summary>
        public string? ContainsName { get; set; }

        /// <summary>
        /// List filter for name.
        /// </summary>
        public string? ListName { get; set; }

        /// <summary>
        /// Date created from (GreaterThanOrEqual).
        /// </summary>
        public DateTime? GreaterThanOrEqualDateCreated { get; set; }

        /// <summary>
        /// Date created to (LessThanOrEqual).
        /// </summary>
        public DateTime? LessThanOrEqualDateCreated { get; set; }

        /// <summary>
        /// Date created greater than (GreaterThan).
        /// </summary>
        public DateTime? GreaterThanDateCreated { get; set; }

        /// <summary>
        /// Date created less than (LessThan).
        /// </summary>
        public DateTime? LessThanDateCreated { get; set; }
    }
}
