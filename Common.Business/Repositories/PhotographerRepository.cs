
using Common.Core.Data.Context;
using Common.Core.Generic.Repository;
using Common.Domain.Entities;

namespace Common.Business.Repositories
{
    public class PhotographerRepository : BaseRepository<Photographer>
    {
        public PhotographerRepository(AppDbContext ctx) : base(ctx)
        {
        }
    }

}
