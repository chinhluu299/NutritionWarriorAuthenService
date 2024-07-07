using NutritionWarriorAuthentication.Data.Entities;
using NutritionWarriorAuthentication.ViewModel.System.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NutritionWarriorAuthentication.Application.System.Auth
{
    public interface IAuthService
    {
        Task<object> SignIn(string email, string password);
        Task<string> RefreshToken(string userId, string refreshToken);
        Task<string> CreateConfirmCode(Guid userId);
        Task<bool> SignOut(string userId, string refreshToken);
        Task<int> ResetPassword(string userId, string confirmCode, string newPassword);
        Task<int> SignUp(string email, string password, string phone, string fullname);
        Task<ClaimsPrincipal> Verify(string token);
    }
}
