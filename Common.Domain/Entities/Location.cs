using Common.Core.Data.Interfaces;

namespace Common.Domain.Entities
{
    public class Location : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public ICollection<Session> Sessions { get; set; }
    }


}
