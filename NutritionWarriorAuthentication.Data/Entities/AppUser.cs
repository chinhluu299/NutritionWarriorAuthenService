using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutritionWarriorAuthentication.Data.Entities
{
    public class AppUser : IdentityUser<Guid>
    {
        public string DisplayName { get; set; }
        public DateTime Dob { get; set; }
        public int Gender { get; set; }
        public string? Address { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActivated { get; set; }
    }
}
