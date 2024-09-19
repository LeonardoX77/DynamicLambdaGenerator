using Common.WebApi.Application;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Reflection;
using Common.WebApi.Application.Mappings;
using Serilog;
using System.Diagnostics;
using Common.Core.Data.Context;

//Debugger.Launch();

const string CORS_POLICY = "CorsPolicy";
var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureSerilog(builder.Configuration);

// Add services to the container.
builder.Services.AddControllers();

builder.SetupConfiguration();

builder.Services.AddMvc()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        //ignore null properties during serialization
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });


builder.Services.AddEndpointsApiExplorer()
    .AddSwagger(builder.Configuration, Assembly.GetExecutingAssembly().GetName().Name)
    .AddConfigurations(builder.Configuration)
    .RegisterServices()
    .AddAutoMapper(expression =>
    {
        expression.AddProfile(new MappingProfile());
    })
    .AddPersistence(builder.Configuration)
    .AddAuthentication(builder.Configuration)
    .SetCorsPolicy(builder.Configuration, CORS_POLICY)
    .AddHttpContextAccessor()
    .AddSecurityResponseHeadersMiddleware();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "dev" || Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "local")
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseForwardedHeaders()
    .ConfigureGlobalExceptionHandler()
    .HandleMigrations(args, builder.Configuration)
    .UseCors(CORS_POLICY)
    .UseSecurityResponseHeaders()
    .UseAuthentication()
    .UseAuthorization();

app.MapControllers();

await DesignTimeAppDbContextFactory.SeedData(app.Services);

app.Run();

