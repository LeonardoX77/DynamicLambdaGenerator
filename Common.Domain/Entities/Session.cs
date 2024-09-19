using Common.Core.Data.Interfaces;

namespace Common.Domain.Entities
{
    public class Session : IEntity
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string SessionType { get; set; }
        public string Notes { get; set; }
        public int ClientId { get; set; }
        public Client Client { get; set; }
        public int PhotographerId { get; set; }
        public Photographer Photographer { get; set; }
        public int LocationId { get; set; }
        public Location Location { get; set; }
    }


}
