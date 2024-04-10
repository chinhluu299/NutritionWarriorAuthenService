using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using NutritionWarriorAuthentication.Data.Entities;
using Microsoft.AspNetCore.Identity;
using NutritionWarriorAuthentication.Data.Configurations;
using NutritionWarriorAuthentication.Data.Extensions;

namespace NutritionWarriorAuthentication.Data.EF
{
    public class NWAuthenDbContext : IdentityDbContext<AppUser, AppRole, Guid>
    {
        public NWAuthenDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {            
            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new AppRoleConfiguration());
            builder.ApplyConfiguration(new AppUserConfiguration());

            builder.Entity<IdentityUserClaim<Guid>>().ToTable("AppUserClaims");
            builder.Entity<IdentityUserRole<Guid>>().ToTable("AppUserRoles").HasKey(x => new { x.UserId, x.RoleId });
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("AppUserLogins").HasKey(x => x.UserId);
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("AppRoleClaims");
            builder.Entity<IdentityUserToken<Guid>>().ToTable("AppUserTokens").HasKey(x => x.UserId);

            builder.Seed();
        }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<AppRole> AppRoles { get; set; }

    }

    public class NWAuthenDbContextFactory : IDesignTimeDbContextFactory<NWAuthenDbContext>
    {
        public NWAuthenDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                                    .SetBasePath(Directory.GetCurrentDirectory())
                                    .AddJsonFile("appsettings.json")
                                    .Build();
            var connectionString = configuration.GetConnectionString("NWAuthenDatabase");

            var optionBuilder = new DbContextOptionsBuilder<NWAuthenDbContext>();
            optionBuilder.UseSqlServer(connectionString);

            return new NWAuthenDbContext(optionBuilder.Options);
        }
    }
}
