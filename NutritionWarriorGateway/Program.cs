using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NutritionWarriorGateway.Middlewares;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace NutritionWarriorGateway
{
    public class Program
    {
        public static async void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
                .AddJsonFile("ocelot.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            builder.Services.AddOcelot(builder.Configuration);

            // Configure CORS
            builder.Services
                .AddCors(options =>
                {
                    options.AddPolicy("CorsPolicy", policy =>
                    {
                        policy
                            //.WithOrigins(builder.Configuration.GetSection("CORS:Origins").Get<string[]>() ?? [])
                            .AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowCredentials()
                            .AllowAnyHeader()
                            .SetIsOriginAllowedToAllowWildcardSubdomains();
                    });
                });

            // Configure health checks
            builder.Services
                .AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

           
            var app = builder.Build();
            app.UseMiddleware<RequestResponseLoggingMiddleware>();

            await app.UseOcelot();

            // Map health check endpoints
            app.MapHealthChecks("/hc", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            });

            app.MapHealthChecks("/liveness", new HealthCheckOptions
            {
                Predicate = r => r.Name.Contains("self")
            });

            app.Run();
        }
    }
}
