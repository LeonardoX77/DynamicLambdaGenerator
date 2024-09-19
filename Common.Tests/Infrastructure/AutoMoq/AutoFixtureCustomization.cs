using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Common.Core.Data.Interfaces;
using Common.Core.Generic.DynamicQueryFilter.DynamicExpressions;
using System.Reflection;

namespace Common.Tests.Infrastructure.AutoMoq
{
    /// <summary>
    /// Customizes the Fixture to register necessary configurations and services.
    /// </summary>
    public class AutoFixtureCustomization : ICustomization
    {
        private readonly IOptions<DynamicFiltersConfiguration> _options;

        public AutoFixtureCustomization() { }

        /// <summary>
        /// Customizes the Fixture to register necessary configurations and services.
        /// </summary>
        /// <param name="fixture">The Fixture to customize.</param>
        public void Customize(IFixture fixture)
        {
            fixture.Register(GetDynamicFiltersConfiguration);
            fixture.Register(GetIConfiguration);
            fixture.Register(CreateContext);
            fixture.Register(CreateMapper);

            fixture.Customize(new AutoMoqCustomization());
        }

        /// <summary>
        /// Retrieves the DynamicFiltersConfiguration from the configuration.
        /// </summary>
        /// <returns>The IOptions<DynamicFiltersConfiguration> instance.</returns>
        private IOptions<DynamicFiltersConfiguration> GetDynamicFiltersConfiguration()
        {
            var configuration = GetIConfiguration();

            var services = new ServiceCollection();
            AddConfigurations(services, configuration);
            var serviceProvider = services.BuildServiceProvider();
            var config = serviceProvider.GetService<IOptions<DynamicFiltersConfiguration>>();

            return config!;
        }

        /// <summary>
        /// Adds the DynamicFiltersConfiguration to the IServiceCollection.
        /// </summary>
        /// <param name="services">The IServiceCollection instance.</param>
        /// <param name="config">The IConfiguration instance.</param>
        /// <returns>The updated IServiceCollection instance.</returns>
        public static IServiceCollection AddConfigurations(IServiceCollection services, IConfiguration config)
        {
            services.Configure<DynamicFiltersConfiguration>(config.GetSection(nameof(DynamicFiltersConfiguration)));
            return services;
        }

        /// <summary>
        /// Creates an instance of IUnitOfWork using an in-memory database context.
        /// </summary>
        /// <returns>The IUnitOfWork instance.</returns>
        private IUnitOfWork CreateContext()
        {
            var dbContextOptions = new DbContextOptionsBuilder<TestDemoContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString(), opt => opt.EnableNullChecks(false))
                .Options;

            return new TestDemoContext(dbContextOptions, _options);
        }

        /// <summary>
        /// Creates an instance of IMapper configured with the assembly of the WebAPI project.
        /// </summary>
        /// <returns>The IMapper instance.</returns>
        public IMapper CreateMapper()
        {
            var assemblyName = "Common.WebApi";
            var webApiAssembly = Assembly.Load(assemblyName);

            if (webApiAssembly == null)
            {
                throw new InvalidOperationException("WebAPI assembly not found.");
            }

            return new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile(webApiAssembly))).CreateMapper();
        }

        /// <summary>
        /// Retrieves the application configuration from appsettings.json and environment variables.
        /// </summary>
        /// <returns>The IConfiguration instance.</returns>
        private static IConfiguration GetIConfiguration()
            => new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
    }
}
