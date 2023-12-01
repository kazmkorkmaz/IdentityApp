using Microsoft.AspNetCore.Mvc;
using IdentityApp.ViewModels;
using Newtonsoft.Json;
using System.Text;
using IdentityAPI.Shared;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;



namespace IdentityClient.Controllers;

public class AuthenticationController : Controller
{

    private IConfiguration _configuration;
    public AuthenticationController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        try
        {
            using (var client = new HttpClient())
            {
                var newUser = new RegisterViewModel
                {
                    Email = model.Email,
                    Password = model.Password,
                    ConfirmPassword = model.ConfirmPassword
                };

                var json = JsonConvert.SerializeObject(newUser);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var apiUrl = _configuration["AuthAPI"];
                var response = await client.PostAsync($"{apiUrl}/register", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<Response>(responseBody);
                    ModelState.AddModelError(string.Empty, responseObject!.Errors!.FirstOrDefault() ?? "An error occurred during registration. Please try again.");
                }
            }
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "An error occurred during registration. Please try again.");
        }

        return View(model);
    }


    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        try
        {
            using (var client = new HttpClient())
            {
                var user = new LoginViewModel
                {
                    Email = model.Email,
                    Password = model.Password,
                };

                var json = JsonConvert.SerializeObject(user);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var apiUrl = _configuration["AuthAPI"];
                var response = await client.PostAsync($"{apiUrl}/login", content);
                var responseBody = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<Response>(responseBody);

                if (response.IsSuccessStatusCode)
                {
                    var jwtToken = responseObject!.Message;
                    Response.Cookies.Append("MyLovelyToken", jwtToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                    });
                    var handler = new JwtSecurityTokenHandler();
                    var jsonToken = handler.ReadToken(jwtToken) as JwtSecurityToken;
                    if (jsonToken != null)
                    {
                        var claims = jsonToken.Claims;
                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var claimsPrincipal = new ClaimsPrincipal(identity);
                        var authProperties = new AuthenticationProperties
                        {
                            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30),
                            IsPersistent = false,
                            AllowRefresh = false,
                        };
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authProperties);
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {

                    ModelState.AddModelError(string.Empty, responseObject!.IsValid == false ? responseObject.Message : responseObject!.Errors!.FirstOrDefault() ?? "An error occurred during login. Please try again.");
                }
            }
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
        }

        return View(model);

    }
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    public IActionResult ConfirmEmail()
    {
        return View();
    }
    public IActionResult ForgetPassword()
    {
        return View();
    }
    public IActionResult ForgetPasswordInfo()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ForgetPassword(string email)
    {
        try
        {
            using (var client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject("");
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var apiUrl = _configuration["AuthAPI"];
                var response = await client.PostAsync($"{apiUrl}/forgetpassword?email={Uri.EscapeDataString(email)}", content);
                var responseBody = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<Response>(responseBody);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("ForgetPasswordInfo", "Authentication");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, responseObject!.Errors!.FirstOrDefault() ?? "An error occurred. Please try again.");
                }
            }
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "An error occurred. Please try again.");
        }

        return View();
    }

    public IActionResult ResetPassword(string email, string token)
    {
        if (email == null || token == null)
        {
            return RedirectToAction("Login");
        }

        var model = new ResetPasswordViewModel { Email = email, Token = token };
        return View(model);
    }
    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        try
        {
            using (var client = new HttpClient())
            {


                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var apiUrl = _configuration["AuthAPI"];
                var response = await client.PostAsync($"{apiUrl}/resetpassword", content);
                var responseBody = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<Response>(responseBody);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Login", "Authentication");
                }
                {

                    ModelState.AddModelError(string.Empty, responseObject!.IsValid == false ? responseObject.Message : responseObject!.Errors!.FirstOrDefault() ?? "An error occurred. Please try again.");
                }
            }
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "An error occurred. Please try again.");
        }
        return View(model);

    }
}
