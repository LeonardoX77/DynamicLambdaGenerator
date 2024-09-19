using Common.Core.Data.Interfaces;

namespace Common.WebApi.Application.Models.Client
{
#nullable enable
    public class ClientResponseDto : IEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime DateCreated { get; set; }
        public string? Comments { get; set; }
        public DateTime BirthDate { get; set; }
        public ICollection<SessionResponseDto>? Sessions { get; set; }
    }
}
