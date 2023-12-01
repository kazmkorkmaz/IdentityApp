using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using IdentityClient.Models;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using IdentityAPI;
using System.Net.Http.Headers;


namespace IdentityClient.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private IConfiguration _configuration;

    public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<IActionResult> Index()
    {
        using (HttpClient httpClient = new HttpClient())
        {
            var apiUrl = _configuration["WeatherAPI"];
            var jwtToken = HttpContext.Request.Cookies["MyLovelyToken"];
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            HttpResponseMessage response = await httpClient.GetAsync($"{apiUrl}/WeatherForecast");
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                IEnumerable<WeatherForecast>? forecastList = JsonConvert.DeserializeObject<IEnumerable<WeatherForecast>>(responseBody);
                return View(forecastList);
            }
        }
        return RedirectToAction("Login", "Authentication");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
