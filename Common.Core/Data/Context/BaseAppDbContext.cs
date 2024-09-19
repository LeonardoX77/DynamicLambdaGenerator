using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Options;
using Common.Core.Data.Identity;
using Common.Core.Data.Interfaces;
using System.Reflection;
using Common.Core.Generic.DynamicQueryFilter.DynamicExpressions;

namespace Common.Core.Data.Context
{
    /// <summary>
    /// Implementacion del DbContext y del contexto de demo.
    /// </summary>
    public class BaseAppDbContext : IdentityDbContext<ApplicationUserBase, ApplicationRole, string>, IUnitOfWork
    {
        private readonly DynamicFiltersConfiguration _options;

        /// <summary>
        /// Constructor.
        /// </summary>
        public BaseAppDbContext() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dbContextOptions"></param>
        public BaseAppDbContext(DbContextOptions dbContextOptions, IOptions<DynamicFiltersConfiguration> options) : base(dbContextOptions)
        {
            _options = options.Value;
        }

        public virtual DbSet<T> GetDbSet<T>() where T : class
        {
            return Set<T>();
        }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            ApplyEntityConfigurations(builder);

        }

        /// <summary>
        /// Calls modelBuilder.ApplyConfiguration dynamically for all classes that implement IEntityTypeConfiguration<T> interface
        /// It dynamically applies all entity configurations that implement the IEntityTypeConfiguration<T> interface. This approach centralizes 
        /// the application of configurations and enhances maintainability by automatically detecting and applying all configuration classes.
        /// </summary>
        /// <param name="modelBuilder"></param>
        private void ApplyEntityConfigurations(ModelBuilder modelBuilder)
        {
            var configurations = Assembly.Load(_options.AssemblyModelName).GetTypes()
                .Where(t => t.GetInterfaces().Any(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)))
                .ToList();

            foreach (var configuration in configurations)
            {
                var configInstance = Activator.CreateInstance(configuration);

                var entityType = configuration.GetInterfaces()
                    .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
                    .GetGenericArguments()[0];

                var applyConfigurationMethod = typeof(ModelBuilder)
                    .GetMethod(nameof(ModelBuilder.ApplyConfiguration), 1, [typeof(IEntityTypeConfiguration<>).MakeGenericType(entityType)]);

                // call ApplyConfiguration method
                applyConfigurationMethod?
                    .MakeGenericMethod(entityType)
                    .Invoke(modelBuilder, [configInstance]);
            }
        }

        /// <inheritdoc />
        public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => await base.SaveChangesAsync(cancellationToken);

        /// <inheritdoc />
        public Task<bool> CanConnectAsync()
        {
            return base.Database.CanConnectAsync();
        }
    }
}
