using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutritionWarriorAuthentication.ViewModel.System.Auth
{
    public class RegisterRequest
    {
        public string email { get; set;}
        public string password { get; set;}
        public string phone_number { get; set;}
        public string name { get; set;}
    }
}
