
using Common.Core.Data.Context;
using Common.Core.Generic.Repository;
using Common.Domain.Entities;

namespace Common.Business.Repositories
{
    public class SessionRepository : BaseRepository<Session>
    {
        public SessionRepository(AppDbContext ctx) : base(ctx)
        {
        }
    }

}
