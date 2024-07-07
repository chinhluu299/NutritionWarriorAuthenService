using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NutritionWarriorAuthentication.Application.Common;
using NutritionWarriorAuthentication.Application.System.Cache;
using NutritionWarriorAuthentication.Data.EF;
using NutritionWarriorAuthentication.Data.Entities;
using NutritionWarriorAuthentication.ViewModel.System.Auth;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NutritionWarriorAuthentication.Application.System.Auth
{
    public class AuthService : IAuthService
    {
        
        public readonly UserManager<AppUser> _userManager;
        public readonly RoleManager<AppRole> _roleManager;
        public readonly SignInManager<AppUser> _signInManager;
        private readonly NWAuthenDbContext _context;
        private readonly JwtService _jwtService;
        private readonly CacheService _cacheService;

        public AuthService(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager,
            SignInManager<AppUser> signInManager, NWAuthenDbContext context, JwtService jwtService,CacheService cacheService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _context = context;
            _jwtService = jwtService;
            _cacheService = cacheService;
        }

        public async Task<string> CreateConfirmCode(Guid userId)
        {
            var expiredAt = DateTime.Now.AddMinutes(2);

            var _token = Math.Floor(new Random().Next(900000) + 100000 * 1.0);
            var setCode = _cacheService.SetData<string>(string.Format("code:{0}", userId), _token.ToString(), expiredAt);
            if (setCode)
            {
                return _token.ToString();
            }
          
            return null;
        }

        public async Task<string> RefreshToken(string accessToken, string refreshToken)
        {
            try
            {
                var getClaims = _jwtService.GetPrincipalFromExpiredToken(accessToken);
                if (getClaims == null)
                    return null;
                var userId = getClaims.FindFirst(JwtRegisteredClaimNames.Name)?.Value;
                var role = getClaims.FindFirst("Role")?.Value;

                int checkToken = _cacheService.GetData<int>(string.Format("token:{0}:{1}", userId, refreshToken));

                if (checkToken == 1)
                {
                    Claim[] claims = new[] {
                        new Claim(JwtRegisteredClaimNames.Name, userId),
                        new Claim("Role", role),
                    };
                    string token = _jwtService.Generate(claims);
                    return token;
                }
                return null;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
        }

        public async Task<ClaimsPrincipal> Verify(string token)
        {
            return _jwtService.Verify(token);
        }

        public async Task<int> ResetPassword(string email, string confirmCode, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return -1;
            }

            var userId = user.Id;

            string checkCode = _cacheService.GetData<string>(string.Format("code:{0}", userId));

            if (checkCode == confirmCode)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                if(_userManager.ResetPasswordAsync(user, token, newPassword).Result == IdentityResult.Success)
                {
                    return 1;
                }
            }
            return 0;
        }

        public async Task<object> SignIn(string email, string password)
        {
            var account = await _userManager.FindByEmailAsync(email);
            if (account == null) return null;
            var checkLogin = await _userManager.CheckPasswordAsync(account, password);
            if (checkLogin == false) return null;

            var roleL = await _userManager.GetRolesAsync(account);

            string role = roleL.Count <= 0 ? "User" : roleL[0];

            Claim[] claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Name, account.Id.ToString()),
                new Claim("Role", role)
            };

            var accessToken = _jwtService.Generate(claims);
            var refreshToken = _jwtService.GenerateRefreshToken();

            //Create new refreshtoken
            bool saveResult = _cacheService.SetData<int>(string.Format("token:{0}:{1}", account.Id, refreshToken),
                1, DateTime.Now.AddDays(7));
            //var role = _userManager.GetRolesAsync(account);
            //------------------
            if(saveResult)
                return new
                {
                    accessToken = accessToken,
                    refreshToken = refreshToken
                };
            else
            {
                return null;
            }
        }

        public async Task<bool> SignOut(string userId, string refreshToken)
        {
            var account = await _userManager.FindByIdAsync(userId);
            if(account == null) return false;

            _cacheService.RemoveData(string.Format("token:{0}:{1}", account.Id, refreshToken));
            return true;
        }

        public async Task<int> SignUp(string email, string password, string phone, string fullname)
        {
            if (await _userManager.FindByEmailAsync(email) != null)
                return -1;
            AppUser user = new AppUser()
            {
                Email = email,
                PhoneNumber = phone,
                DisplayName = fullname,
                UserName = email,

            };
            var result = _userManager.CreateAsync(user, password).Result;
            if ( result == IdentityResult.Success)
            {
                var newUser = await _userManager.FindByEmailAsync(email);
                await _userManager.AddToRoleAsync(newUser, "User");
                return 1;
            }


            return 0;
            
        }
    }
}
