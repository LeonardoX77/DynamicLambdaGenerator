using Common.WebApi.Infrastructure.Models.Request;
using Common.Core.Generic.DynamicQueryFilter.Interfaces;

namespace Common.WebApi.Application.Models.Session
{
#nullable enable

    /// <summary>
    /// Filter for dynamic queries
    /// </summary>
    public class SessionQueryFilter : SessionDynamicFieldsQueryFilter
    {
        public int? Id { get; set; }
        public DateTime? Date { get; set; }
        public TimeSpan? Time { get; set; }
        public string? SessionType { get; set; }
        public string? Notes { get; set; }
        public int? ClientId { get; set; }
        public int? PhotographerId { get; set; }
        public int? LocationId { get; set; }
    }
}
