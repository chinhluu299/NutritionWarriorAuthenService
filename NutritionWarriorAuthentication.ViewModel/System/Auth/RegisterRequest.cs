using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutritionWarriorAuthentication.ViewModel.System.Auth
{
    public class RegisterRequest
    {
        public string Email { get; set;}
        public string Password { get; set;}
        public DateTime Dob { get; set;}
        public int Gender { get; set; }

        public string FullName { get; set;}
    }
}
