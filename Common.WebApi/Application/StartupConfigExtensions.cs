using Common.WebApi.Application.Middlewares;
using Common.WebApi.Infrastructure.Settings;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Common.Business.Services.Common;
using Common.WebApi.Application.Services.Interfaces;
using Common.Core.Data.Context;
using Common.Core.Generic.Services;
using Common.WebApi.Application;
using Common.Core.Data.Interfaces;
using Common.Domain.Entities;
using Common.Business.Services;
using Common.Core.Data.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Common.Core.CustomExceptions;
using FluentValidation;
using Serilog;
using Common.Business.Repositories;
using Common.Core.Generic.DynamicQueryFilter.DynamicExpressions;
using Common.Core.Generic.Controllers;
using Common.Core.Generic.Controllers.Response;

namespace Common.WebApi.Application
{
    /// <summary>
    /// Methods to extend the application configuration.
    /// </summary>
    public static class StartupConfigExtensions
    {
        /// <summary>
        /// Adds new configurations to the WebApplicationBuilder.
        /// </summary>
        /// <param name="builder">Application builder.</param>
        /// <returns>WebApplicationBuilder.</returns>
        /// <exception cref="InvalidOperationException">Invalid operation.</exception>
        public static WebApplicationBuilder SetupConfiguration(this WebApplicationBuilder builder)
        {
            var environment = (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "").ToLower();
            var appSettingsFileEnvName = !string.IsNullOrWhiteSpace(environment) ? "appsettings." + environment + ".json" : null;

            Console.WriteLine("#################################################################");
            if (!string.IsNullOrWhiteSpace(environment))
                Console.WriteLine("Detected Env Var ASPNETCORE_ENVIRONMENT = " + environment);

            else throw new InvalidOperationException($"{nameof(SetupConfiguration)} - No ASPNETCORE_ENVIRONMENT variable found. Exiting program ...");
            Console.WriteLine("#################################################################");

            Console.WriteLine("DBConnectionString = " + Environment.GetEnvironmentVariable("DBConnectionString"));
            Console.WriteLine("Configuration source for environment is:");
            Console.WriteLine("  - appsettings.json " + (File.Exists("appsettings.json") ? "(present)" : "(not found)"));

            if (appSettingsFileEnvName != null)
                Console.WriteLine("  - " + appSettingsFileEnvName + " " + (File.Exists(appSettingsFileEnvName) ? "(present)" : "(not found)"));

            Console.WriteLine("  - environment variables");

            builder.Configuration
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile(appSettingsFileEnvName, optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            return builder;
        }

        #region App Services

        /// <summary>
        /// Adds some service configurations to IServiceCollection.
        /// </summary>
        /// <param name="services">Application service collection.</param>
        /// <param name="config">Application configuration.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection AddConfigurations(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<SecurityHeaders>(config.GetSection(nameof(SecurityHeaders)));
            services.Configure<DynamicFiltersConfiguration>(config.GetSection(nameof(DynamicFiltersConfiguration)));

            return services;
        }

        /// <summary>
        /// Registers application services in IServiceCollection.
        /// </summary>
        /// <param name="services">Application service collection.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            // Register repositories
            services.AddScoped<IBaseService<Client, int>, ClientService>();
            services.AddScoped<IBaseService<Location, int>, LocationService>();
            services.AddScoped<IBaseService<Photographer, int>, PhotographerService>();
            services.AddScoped<IBaseService<Session, int>, SessionService>();

            services.AddScoped<IRepository<Client>, ClientRepository>();
            services.AddScoped<IRepository<Location>, LocationRepository>();
            services.AddScoped<IRepository<Photographer>, PhotographerRepository>();
            services.AddScoped<IRepository<Session>, SessionRepository>();

            services.AddScoped<IValidationService, ValidationService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ISeedManager, SeedManager>();

            return services;
        }

        /// <summary>
        /// Adds persistence configurations to IServiceCollection.
        /// </summary>
        /// <param name="services">Application service collection.</param>
        /// <param name="configuration">Application configuration.</param>
        /// <returns></returns>
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            Console.WriteLine("DBConnectionString: " + configuration["DbConnectionString"]);

            services.AddDbContext<AppDbContext>(o => o.UseSqlServer(configuration["DbConnectionString"]));

            services.AddIdentity<ApplicationUserBase, ApplicationRole>()
                    .AddEntityFrameworkStores<AppDbContext>()
                    .AddDefaultTokenProviders();

            return services;
        }

        /// <summary>
        /// Adds security headers to responses.
        /// </summary>
        /// <param name="services">Application service collection.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection AddSecurityResponseHeadersMiddleware(this IServiceCollection services)
        {
            return services
                .AddTransient<SecurityHeadersMiddleware>()
                .AddHsts(options =>
                {
                    options.Preload = true;
                    options.IncludeSubDomains = true;
                    options.MaxAge = TimeSpan.FromDays(365);
                });
        }

        /// <summary>
        /// Adds authentication configuration to IServiceCollection.
        /// </summary>
        /// <param name="services">Application service collection.</param>
        /// <param name="configuration">Application configuration.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient()
                    // Configure JWT authentication service
                    .AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            // Token issuer
                            ValidateIssuer = true,
                            ValidIssuer = configuration["JwtSettings:Issuer"],

                            // Token audience
                            ValidateAudience = true,
                            ValidAudience = configuration["JwtSettings:Audience"],

                            // Signing key
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"])),

                            // Token expiration
                            ValidateLifetime = true,

                            // Validate token security
                            RequireExpirationTime = true,
                        };
                    });

            return services;
        }

        /// <summary>
        /// Adds Swagger to the application service collection.
        /// </summary>
        /// <param name="services">Application service collection.</param>
        /// <param name="configuration">Application configuration.</param>
        /// <param name="xmlAssemblyName">XML file name.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection AddSwagger(this IServiceCollection services, ConfigurationManager configuration, string xmlAssemblyName)
        {
            var docTitle = configuration.GetValue<string>("ServiceName");

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = docTitle, Version = "v1" });

                c.EnableAnnotations();
                // Config X-Api-Key
                c.AddSecurityDefinition("X-Api-Key", new OpenApiSecurityScheme
                {
                    Description = "ApiKey must appear in header",
                    Type = SecuritySchemeType.ApiKey,
                    Name = "X-Api-Key",
                    In = ParameterLocation.Header,
                    Scheme = "ApiKeyScheme"
                });
                var key = new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "X-Api-Key"
                    },
                    In = ParameterLocation.Header
                };
                var requirement = new OpenApiSecurityRequirement
                {
                    { key, new List<string>() }
                };
                c.AddSecurityRequirement(requirement);
                // Config Token
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Add valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                var filePath = Path.Combine(AppContext.BaseDirectory, $"{xmlAssemblyName}.xml");

                c.IncludeXmlComments(filePath);

                c.OperationFilter<ProducesResponseTypeGenericFilter>();
            });

            return services;
        }

        /// <summary>
        /// Sets the Cors policy for the application.
        /// </summary>
        /// <param name="services">Application service collection.</param>
        /// <param name="configuration">Application configuration.</param>
        /// <param name="policyName">Policy name to use.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection SetCorsPolicy(this IServiceCollection services, IConfiguration configuration, string policyName = "CorsPolicy")
        {
            var authorizedCorsOrigins = configuration.GetSection("Cors:AuthorizedOrigins").Get<List<string>>();

            services.AddCors(options =>
            {
                options.AddPolicy(name: policyName, builder =>
                {
                    builder.WithOrigins(authorizedCorsOrigins.ToArray())
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials()
                           .SetPreflightMaxAge(new TimeSpan(8, 0, 0)) // Cache for 8H
                           .Build();
                });
            });

            return services;
        }

        #endregion

        #region App Builder
        /// <summary>
        /// Configures how to handle some exceptions for the application.
        /// </summary>
        /// <param name="app">Application builder.</param>
        /// <returns>IApplicationBuilder.</returns>
        public static IApplicationBuilder ConfigureGlobalExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(error =>
            {
                using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();

                error.Run(async ctx =>
                {
                    ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    ctx.Response.ContentType = "application/json";

                    var contextFeature = ctx.Features.Get<IExceptionHandlerFeature>();

                    if (contextFeature != null)
                    {
                        int errorCode;
                        string errorTag = string.Empty;

                        StringBuilder logTrace = new();

                        logTrace.Append(contextFeature.Error.Message);

                        if (contextFeature.Error is CustomException customException)
                        {
                            ctx.Response.StatusCode = customException.HttpStatusCodeResponse;
                            errorCode = customException.ErrorCode;
                            errorTag = customException.ErrorTag;

                            logTrace.Append(customException.InnerException != null ? " :: Inner Exception: " + customException.InnerException.Message : "");
                        }
                        else if (contextFeature.Error is ValidationException fluentValidationException)
                        {
                            var error = fluentValidationException.Errors.FirstOrDefault();

                            ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                            errorCode = Convert.ToInt32(error.ErrorCode);
                            errorTag = Enum.GetName(typeof(ApiErrorCode), errorCode);
                        }
                        else
                        {
                            ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            errorCode = (int)HttpStatusCode.InternalServerError;
                            errorTag = nameof(ApiErrorCode.INTERNAL_SERVER_ERROR);
                        }

                        logTrace.Append(contextFeature.Error.InnerException != null ? " :: Inner Exception: " + contextFeature.Error.InnerException.Message : "");

                        logTrace.Append(" :: StackTrace :: ")
                                .Append(contextFeature.Error.StackTrace);

                        Log.Error(logTrace.ToString());

                        var apiError = new ApiError(errorCode, $"{contextFeature.Error.Message}");

                        var apiResponse = new Response<string>(apiError);

                        await ctx.Response.WriteAsync(apiResponse.ToString());
                    }
                });
            });

            return app;
        }

        /// <summary>
        /// Tries to return some migration options based on the command line arguments the application starts with.
        /// According to the options it tries:
        ///     - Execute a specific migration by name
        ///     - Update to the latest migration if there are any pending
        ///     - Check and log the number and name of pending migrations
        ///     
        /// If there are no options or no commands are passed, it tries to execute the latest pending migration if it finds any
        /// In case of an exception, it logs the trace and closes the application.
        /// </summary>
        /// <param name="app">Application builder.</param>
        /// <param name="args">CommandLine arguments</param>
        /// <param name="config"></param>
        /// <returns>IApplicationBuilder</returns>
        public static IApplicationBuilder HandleMigrations(this IApplicationBuilder app, string[] args, ConfigurationManager config)
        {
            using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();

            Log.Information($"{nameof(HandleMigrations)}");

            try
            {
                var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>() ?? throw new InvalidOperationException($"{nameof(HandleMigrations)} - DbContext of type {nameof(AppDbContext)} not found. Exiting program ...");
                bool? migrationCommandOption = config.GetValue<bool?>("CheckMigrations");

                if (migrationCommandOption.HasValue && migrationCommandOption.Value)
                {
                    StringBuilder sb = new StringBuilder()
                        .Append("Validating Entity Framework Migrations state")
                        .Append('\t');

                    var pendingMigrations = ctx.Database.GetPendingMigrations();

                    if (pendingMigrations.Any())
                    {
                        sb.Append($"Pending migrations: [{pendingMigrations.Count()}]");

                        pendingMigrations.ToList().ForEach(migrationName => sb.Append('\t').Append(migrationName));
                    }

                    else sb.Append("No pending migrations");

                    Log.Information(sb.ToString());

                    ctx.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex.ToString());

                Environment.Exit(-1);
            }

            return app;
        }

        /// <summary>
        /// Finds the latest pending migration and executes it if there is one
        /// In case of an exception, logs the trace and closes the application.
        /// </summary>
        /// <typeparam name="T">DbContext subclass.</typeparam>
        /// <returns>IApplicationBuilder.</returns>
        public static IApplicationBuilder HandleMigrations<T>(this IApplicationBuilder app) where T : DbContext
        {
            using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();

            try
            {
                var ctx = scope.ServiceProvider.GetRequiredService<T>();

                Log.Information($"{nameof(HandleMigrations)} for app context: {ctx.GetType().Name}");

                if (ctx.Database.GetPendingMigrations().Any())
                {
                    Log.Information($"{nameof(HandleMigrations)} - Pending migrations found. Updating context to latest migration ...");

                    ctx.Database.Migrate();
                }

            }
            catch (Exception ex)
            {
                Log.Fatal(ex.ToString());

                Environment.Exit(-1);
            }

            return app;
        }

        public static IApplicationBuilder UseSecurityResponseHeaders(this IApplicationBuilder app)
        {
            app
            .UseMiddleware<SecurityHeadersMiddleware>()
            .UseHsts(); //by default excludes localhost > no need to condition it.

            return app;
        }

        public static IHostBuilder ConfigureSerilog(this IHostBuilder host, IConfiguration config)
        {
            Console.WriteLine($"{nameof(ConfigureSerilog)} - Configuring Serilog ...");

            Log.Logger = new LoggerConfiguration()
                .CreateBootstrapLogger()
                .ForContext<Program>();

            host
            .ConfigureLogging(logging => logging.ClearProviders())
            .UseSerilog((context, logConfig) =>
            {
                logConfig
                .ReadFrom.Configuration(config)
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .Enrich.WithProcessId()
                .Enrich.WithProperty("ClientHost", Dns.GetHostAddresses(Dns.GetHostName())[0].ToString());
            });

            return host;
        }

        #endregion
    }
}
