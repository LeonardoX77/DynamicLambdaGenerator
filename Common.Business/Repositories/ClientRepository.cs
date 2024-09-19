
using Common.Core.Data.Context;
using Common.Core.Generic.Repository;
using Common.Domain.Entities;

namespace Common.Business.Repositories
{
    public class ClientRepository : BaseRepository<Client>
    {
        public ClientRepository(AppDbContext ctx) : base(ctx)
        {
        }
    }

}
