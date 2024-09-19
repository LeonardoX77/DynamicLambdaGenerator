using Common.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Common.Core.Data.Context;
using Microsoft.Extensions.Logging;
using Common.Core.Generic.DynamicQueryFilter.DynamicExpressions;


namespace Common.Core.Data.Context
{
    /// <summary>
    /// Implementacion del DbContext y del contexto de demo.
    /// </summary>
    public class AppDbContext : BaseAppDbContext
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public AppDbContext() : base() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="options">Opciones del DbContext.</param>
        public AppDbContext(
            DbContextOptions dbContextOptions, 
            IOptions<DynamicFiltersConfiguration> options) : base(dbContextOptions, options) 
        { 
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Photographer> Photographers { get; set; }
        public DbSet<Session> Sessions { get; set; }
    }
}
