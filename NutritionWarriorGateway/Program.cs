using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using NutritionWarriorGateway.Middlewares;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Values;
using System.Security.Claims;
using System.Text;

namespace NutritionWarriorGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
                .AddJsonFile("ocelot.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            builder.Services.AddControllers();
            builder.Services.AddOcelot(builder.Configuration);
            builder.Services.AddSwaggerForOcelot(builder.Configuration);
            builder.Services.AddSwaggerGen();
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
                            //.AllowCredentials()
                            .AllowAnyHeader()
                            .SetIsOriginAllowedToAllowWildcardSubdomains();
                    });
                });
            //
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Tokens:Issuer"],
                    ValidAudience = builder.Configuration["Tokens:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Tokens:Key"])),
                    LifetimeValidator = (DateTime? notBefore, DateTime? expires, SecurityToken _, TokenValidationParameters __) =>
                    {
                        if (expires != null && expires > DateTime.UtcNow)
                        {
                            return true;
                        }
                        return false;
                    },
                    RoleClaimType = ClaimTypes.Role,
                };
            });
            // Configure health checks
            builder.Services
                .AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

           
            var app = builder.Build();
            app.UseMiddleware<RequestResponseLoggingMiddleware>();
            // Map health check endpoints
            //if (app.Environment.IsDevelopment())
            //{
            
            //}
            app.UseRouting();
            app.UseCors("CorsPolicy");
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
                {
                    Predicate = r => r.Name.Contains("self")
                });
                endpoints.MapSwagger();
            });
            app.UseSwagger();
            app.UseSwaggerForOcelotUI();
            app.UseOcelot().Wait();
            app.MapGet("/", () => "Gate way ok");
            app.Run();
        }
    }
}
