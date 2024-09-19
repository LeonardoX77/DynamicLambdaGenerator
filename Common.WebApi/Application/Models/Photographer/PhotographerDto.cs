
using Common.Core.Data.Interfaces;

namespace Common.WebApi.Application.Models.Photographer
{
#nullable enable
    public class PhotographerDto : IEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
    }

}
