using Common.Core.Data.Interfaces;
using Common.WebApi.Application.Models.Location;
using Common.WebApi.Application.Models.Photographer;

namespace Common.WebApi.Application.Models.Client
{
    public class SessionResponseDto : IEntity
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string SessionType { get; set; }
        public string Notes { get; set; }
        public int ClientId { get; set; }
        public ClientResponseDto Client { get; set; }
        public int PhotographerId { get; set; }
        public PhotographerDto Photographer { get; set; }
        public int LocationId { get; set; }
        public LocationDto Location { get; set; }
    }

}
