
using Common.WebApi.Infrastructure.Models.Request;
using Common.Core.Data.Interfaces;

namespace Common.WebApi.Application.Models.Client
{
#nullable enable

    /// <summary>
    /// Filter for dynamic queries
    /// </summary>
    public class ClientQueryFilter : ClientDynamicFieldsQueryFilter
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
