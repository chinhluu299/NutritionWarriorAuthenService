using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NutritionWarriorAuthentication.Application.System.Auth;
using NutritionWarriorAuthentication.Data.Entities;
using NutritionWarriorAuthentication.ViewModel.System.Auth;
using System.Net.Mail;
using System.Net;
using NutritionWarriorAuthentication.Data.EF;
using NutritionWarriorAuthentication.Api.Queues;

namespace NutritionWarriorAuthentication.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public readonly IAuthService _AuthService;
        public readonly NWAuthenDbContext _context;
        public readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;


        public AuthController(IAuthService authService, NWAuthenDbContext context, UserManager<AppUser> userManager, IConfiguration configuration)
        {
            _AuthService = authService;
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<ActionResult> SignIn([FromBody] LoginRequest request)
        {
            try
            {
                var login = await _AuthService.SignIn(request.Email, request.Password);

                return Ok( new { login });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("refreshtoken")]
        public async Task<ActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (request == null)
            {
                var response = new { message = "Refresh Token is required!" };
                return StatusCode(403, response);
            }
            try
            {
                var accessToken = await _AuthService.RefreshToken(request.AccessToken, request.RefreshToken);
                if (accessToken == null)
                    return BadRequest("Refresh token is not valid. Please make a new signin request");
                
                return Ok(new { accessToken = accessToken });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost("send-mail")]
        public async Task<IActionResult> SendEmail([FromBody] SendEmailRequest request)
        {
            try
            {
                var isExistUser = await _userManager.FindByEmailAsync(request.Email);
                if (isExistUser == null)
                {
                    return BadRequest("User doesn't exist!");
                }

                var email = isExistUser.Email;
                var confirmCode = await _AuthService.CreateConfirmCode(isExistUser.Id);
                var htmlBody = $"<div style=\"padding: 10px; background-color: #003375\">\r\n          " +
                    $"<div style=\"padding: 10px; background-color: white;\">\r\n              " +
                    $"<h4 style=\"color: #0085ff\">Hi {isExistUser.UserName}</h4>\r\n              " +
                    $"<span style=\"color: black\">Your confirm code is {confirmCode}</span>\r\n          " +
                    $"</div>\r\n      </div>";


                var message = new MailMessage();
                message.To.Add(new MailAddress(email));
                message.From = new MailAddress(_configuration["Email:GOOGLE_ACCOUNT"]);
                message.Subject = "Reset Password";
                message.Body = htmlBody;
                message.IsBodyHtml = true;


                using (var smtpClient = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential
                    {
                        UserName = _configuration["Email:GOOGLE_ACCOUNT"],
                        Password = _configuration["Email:GOOGLE_PASSWORD"]
                    };
                    smtpClient.EnableSsl = true;
                    
                    await smtpClient.SendMailAsync(message);
                }

                return Ok($"Send confirm code to {email} successfully");
            }
            catch(Exception ex)
            {
                return StatusCode(500, new { message = "Send code failure" , error=ex.Message});
            }
        }
        [HttpPost("verify")]
        public async Task<IActionResult> Verify([FromBody] string token)
        {
            try {
                var claims = await _AuthService.Verify(token);

                return StatusCode(200,  claims);
            }
            catch(Exception ex) { return StatusCode(400,new {message = "Token is invalid", error=ex.Message }); } 
        }


        [HttpPost("change-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                var username = request.Email;
                var code = request.Code;
                var newPass = request.Password;
               
                int reset = await _AuthService.ResetPassword(username, code, newPass);
                if(reset == -1)
                {
                    return BadRequest(new { message = "User is not found!" });
                }
                if (reset == 0)
                {
                    return BadRequest(new { message = "Confirm code is incorrect!" });
                }        
                  
                return Ok(new { message = "Reset password successfully." });
            }catch (Exception ex)
            {
                return StatusCode(500, new { message = "Reset password failure", error = ex.Message });
            }
        }


        [HttpPost("")]
        public async Task<IActionResult> SignUp([FromBody] RegisterRequest request)
        {
            try
            {
               string result = await _AuthService.SignUp(request.email, request.password, request.phone_number, request.name);
               if(result == "-1")
                    return StatusCode(400, new { message = "Email has already been registered.", success = false });
               if(result != "0")
                {
                    Send.CreateUser(result,request.email, request.phone_number, request.name);
                    return Ok(new { message = "Account is registered successfully", success = true });
                }
                    

                return StatusCode(400, new { message = "Cannot sign up account", success = false });
            }
            catch(Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
