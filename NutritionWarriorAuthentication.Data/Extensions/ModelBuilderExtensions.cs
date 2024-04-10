using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NutritionWarriorAuthentication.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutritionWarriorAuthentication.Data.Extensions
{
    public static class ModelBuilderExtensions
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            var roleId = new Guid("8D04DCE2-969A-435D-BBA4-DF3F325983DC");
            var roleId1 = new Guid("0FCBB353-AE6B-4936-9FDD-950EFEB452A6");
            var roleId2 = new Guid("09480504-4C27-4AF7-A492-ADCDBBE6C097");

            modelBuilder.Entity<AppRole>().HasData(
                new AppRole
                {
                    Id = roleId,
                    Name = "admin",
                    NormalizedName = "admin",
                    Description = "Administrator role"
                },
                new AppRole
                {
                    Id = roleId1,
                    Name = "user",
                    NormalizedName = "user",
                    Description = "User role"
                },
                new AppRole
                {
                    Id = roleId2,
                    Name = "vip",
                    NormalizedName = "vip",
                    Description = "VIP role"
                }
            );
        }
    }
}
