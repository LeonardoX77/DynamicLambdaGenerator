using Common.Core.Data.Interfaces;

namespace Common.WebApi.Application.Models.Location
{
#nullable enable
    public class LocationDto : IEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
    }


}
