using System.IO.Compression;
using IdentityAPI.Models;
using IdentityAPI.Services.Abstract;
using IdentityAPI.Shared;
using Microsoft.AspNetCore.Identity;
using IdentityApp.ViewModels;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace IdentityAPI.Services.EfCore
{
    public class UserService : IUserService
    {
        private UserManager<AppUser> _userManager;
        private IEmailService _emailService;
        private IConfiguration _configuration;
        private SignInManager<AppUser> _signInManager;
        public UserService(UserManager<AppUser> userManager, IConfiguration configuration, IEmailService emailService, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _configuration = configuration;
            _emailService = emailService;
            _signInManager = signInManager;
        }

        public async Task<Response> Login(LoginViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return new Response
                {
                    Message = "User can not find this email address!",
                    IsValid = false,
                };
            }
            await _signInManager.SignOutAsync();

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                return new Response
                {
                    Message = "Authenticate account!",
                    IsValid = false,
                };
            }

            var result = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!result)
            {
                return new Response
                {
                    Message = "Invalid password!",
                    IsValid = false,
                };
            }
            var claims = new[] {
                new Claim("Email", model.Email),
                new Claim(ClaimTypes.NameIdentifier,user.UserName!)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AuthenticationSettings:Key"]!));

            var token = new JwtSecurityToken(
                issuer: _configuration["AuthenticationSettings:Issuer"],
                audience: _configuration["AuthenticationSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(55),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            string tokenAsString = new JwtSecurityTokenHandler().WriteToken(token);

            return new Response
            {
                Message = tokenAsString,
                IsValid = true,
                ExpireDate = token.ValidTo
            };
        }

        public async Task<Response> Register(RegisterViewModel model)
        {
            if (model == null) throw new NullReferenceException("Your data is null!");
            if (model.Password != model.ConfirmPassword)
            {
                return new Response
                {
                    Message = "Passwords are not match!"
                };
            }
            var newUser = new AppUser
            {
                Email = model.Email,
                UserName = model.Email,
            };
            var result = await _userManager.CreateAsync(newUser, model.Password);
            if (result.Succeeded)
            {
                var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);

                var encodedToken = Encoding.UTF8.GetBytes(emailToken);
                var validToken = WebEncoders.Base64UrlEncode(encodedToken);

                string url = $"{_configuration["AppUrl"]}/api/auth/confirmemail?userid={newUser.Id}&token={validToken}";

                await _emailService.SendEmailAsync(newUser.Email, "Confirm your email", $"<h1>Welcome to Auth Demo</h1>" +
                    $"<p>Please confirm your email by <a href='{url}'>Clicking here</a></p>");
                return new Response
                {
                    Message = "User Created",
                    IsValid = true,
                };

            }
            return new Response
            {
                Message = "Registiration is failed!",
                IsValid = false,
                Errors = result.Errors.Select(e => e.Description)
            };
        }

        public async Task<Response> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new Response
                {
                    IsValid = false,
                    Message = "User not found"
                };

            var decodedToken = WebEncoders.Base64UrlDecode(token);
            string normalToken = Encoding.UTF8.GetString(decodedToken);

            var result = await _userManager.ConfirmEmailAsync(user, normalToken);

            if (result.Succeeded)
                return new Response
                {
                    Message = "Email confirmed.",
                    IsValid = true,
                };

            return new Response
            {
                IsValid = false,
                Message = "Email did not confirm",
                Errors = result.Errors.Select(e => e.Description)
            };
        }


        public async Task<Response> ForgetPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return new Response
                {
                    IsValid = false,
                    Message = "No user associated with email",
                };

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = Encoding.UTF8.GetBytes(token);
            var validToken = WebEncoders.Base64UrlEncode(encodedToken);

            string url = $"{_configuration["ClientAppUrl"]}/Authentication/ResetPassword?email={email}&token={validToken}";

            await _emailService.SendEmailAsync(email, "Reset Password", "<h1>Follow the instructions to reset your password</h1>" +
                $"<p>To reset your password <a href='{url}'>Click here</a></p>");

            return new Response
            {
                IsValid = true,
                Message = "Reset password URL has been sent to the email successfully!"
            };
        }

        public async Task<Response> ResetPassword(ResetPasswordViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return new Response
                {
                    IsValid = false,
                    Message = "No user associated with email",
                };

            if (model.NewPassword != model.ConfirmPassword)
                return new Response
                {
                    IsValid = false,
                    Message = "Password doesn't match its confirmation",
                };

            var decodedToken = WebEncoders.Base64UrlDecode(model.Token);
            string normalToken = Encoding.UTF8.GetString(decodedToken);

            var result = await _userManager.ResetPasswordAsync(user, normalToken, model.NewPassword);

            if (result.Succeeded)
                return new Response
                {
                    Message = "Password has been reset successfully!",
                    IsValid = true,
                };

            return new Response
            {
                Message = "Something went wrong",
                IsValid = false,
                Errors = result.Errors.Select(e => e.Description),
            };
        }

    }
}