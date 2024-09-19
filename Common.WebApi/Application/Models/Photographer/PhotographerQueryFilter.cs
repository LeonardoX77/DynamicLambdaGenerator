using Common.WebApi.Infrastructure.Models.Request;
using Common.Core.Generic.DynamicQueryFilter.Interfaces;

namespace Common.WebApi.Application.Models.Photographer
{
#nullable enable

    /// <summary>
    /// Filter for dynamic queries
    /// </summary>
    public class PhotographerQueryFilter : PhotographerDynamicFieldsQueryFilter
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
