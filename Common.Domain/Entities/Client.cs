using Common.Core.Data.Interfaces;

namespace Common.Domain.Entities
{
    public class Client : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Comments { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime BirthDate { get; set; }
        public ICollection<Session> Sessions { get; set; }
    }

}
