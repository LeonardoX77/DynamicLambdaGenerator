using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Common.Core.Data.Interfaces;
using Common.Core.Generic.DynamicQueryFilter.DynamicExpressions;

namespace Common.Core.Data.Context
{
    /// <summary>
    /// This class instantiates AppDbContext during design-time operations like migrations, ensuring that the context is 
    /// configured correctly based on the application's configuration settings.
    /// </summary>
    public class DesignTimeAppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            Console.WriteLine("reading config ...");
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            optionsBuilder.UseSqlServer(configuration["DbConnectionString"]);

            var dynamicFiltersConfig = new DynamicFiltersConfiguration();
            configuration.GetSection("DynamicFiltersConfiguration").Bind(dynamicFiltersConfig);
            var filterOptions = Options.Create(dynamicFiltersConfig);

            var context = new AppDbContext(optionsBuilder.Options, filterOptions);

            return context;
        }

        public static async Task SeedData(IServiceProvider serviceProvider)
{
            using var scope = serviceProvider.CreateScope();
            var seedManager = scope.ServiceProvider.GetRequiredService<ISeedManager>();

            await seedManager.CreateDefaultRolesAsync();
            await seedManager.CreateDefaultUsersAsync();
        }
    }
}