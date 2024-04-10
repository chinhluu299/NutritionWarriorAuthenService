using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NutritionWarriorAuthentication.Application.Common;
using NutritionWarriorAuthentication.Application.System.Auth;
using NutritionWarriorAuthentication.Application.System.Cache;
using NutritionWarriorAuthentication.Data.EF;
using NutritionWarriorAuthentication.Data.Entities;
using System.Security.Claims;
using System.Text;

namespace NutritionWarriorAuthentication.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            IConfiguration configuration = builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            builder.Services.AddCors(option =>
            {
                option.AddPolicy("AllowAnyOrigin", builder => builder.AllowAnyOrigin()
                                                            .AllowAnyHeader()
                                                            .AllowAnyMethod());
            });

            builder.Services.AddDbContext<NWAuthenDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("NWAuthenDatabase"));
            });

            builder.Services.AddIdentity<AppUser, AppRole>()
                .AddEntityFrameworkStores<NutritionWarriorAuthentication.Data.EF.NWAuthenDbContext>()
                .AddDefaultTokenProviders();


            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
                googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
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
            builder.Services.AddSingleton(configuration);
            builder.Services.AddTransient<UserManager<AppUser>, UserManager<AppUser>>();
            builder.Services.AddTransient<SignInManager<AppUser>, SignInManager<AppUser>>();
            builder.Services.AddTransient<RoleManager<AppRole>, RoleManager<AppRole>>();
            builder.Services.AddTransient<JwtService, JwtService>();
            builder.Services.AddTransient<CacheService, CacheService>();
            builder.Services.AddTransient<IAuthService, AuthService>();
            builder.Services.AddTransient<NWAuthenDbContext, NWAuthenDbContext>();
          

            builder.Services.AddControllers();
            
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
           
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
