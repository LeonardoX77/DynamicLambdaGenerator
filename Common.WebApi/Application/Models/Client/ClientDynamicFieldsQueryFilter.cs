using Common.WebApi.Infrastructure.Models.Request;
using Common.Core.Generic.DynamicQueryFilter.Interfaces;
using System;
using System.Collections.Generic;

namespace Common.WebApi.Application.Models.Client
{
#nullable enable

    /// <summary>
    /// Filter for dynamic Fields
    /// </summary>
    public class ClientDynamicFieldsQueryFilter : BaseRequest, IDynamicQueryFilter
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
        public string[]? ListName { get; set; }

        /// <summary>
        /// GreaterThanOrEqual date
        /// </summary>
        public DateTime? GreaterThanOrEqualBirthDate { get; set; }

        /// <summary>
        /// LessThanOrEqual date
        /// </summary>
        public DateTime? LessThanOrEqualBirthDate { get; set; }

        /// <summary>
        /// GreaterThan date
        /// </summary>
        public DateTime? GreaterThanBirthDate { get; set; }

        /// <summary>
        /// LessThan date
        /// </summary>
        public DateTime? LessThanBirthDate { get; set; }

        /// <summary>
        /// GreaterThanOrEqual date
        /// </summary>
        public DateTime? GreaterThanOrEqualDateCreated { get; set; }

        /// <summary>
        /// LessThanOrEqual date
        /// </summary>
        public DateTime? LessThanOrEqualDateCreated { get; set; }

        /// <summary>
        /// GreaterThan date
        /// </summary>
        public DateTime? GreaterThanDateCreated { get; set; }

        /// <summary>
        /// LessThan date
        /// </summary>
        public DateTime? LessThanDateCreated { get; set; }
    }
}
