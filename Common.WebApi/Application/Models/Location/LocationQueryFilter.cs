using Common.WebApi.Infrastructure.Models.Request;
using Common.Core.Generic.DynamicQueryFilter.Interfaces;

namespace Common.WebApi.Application.Models.Location
{
#nullable enable

    /// <summary>
    /// Filter for dynamic queries
    /// </summary>
    public class LocationQueryFilter : LocationDynamicFieldsQueryFilter
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
    }
}
