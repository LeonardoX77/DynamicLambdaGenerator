using Common.Core.Data.Interfaces;

namespace Common.WebApi.Application.Models.Session
{
#nullable enable
    public class SessionRequestDto : IEntity
    {
        public int Id { get; set; }
        public DateTime? Date { get; set; }
        public TimeSpan? Time { get; set; }
        public string? SessionType { get; set; }
        public string? Notes { get; set; }
        public int? ClientId { get; set; }
        public int? PhotographerId { get; set; }
        public int? LocationId { get; set; }
    }


}
