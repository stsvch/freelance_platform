using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace WebApp.Controllers
{
    [Route("profile")]
    public class ProfileController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ProfileController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {

            var userId = HttpContext.Session.GetString("UserId");
            var message = JsonConvert.SerializeObject(new
            {
                UserId = userId
            });
            var client = _httpClientFactory.CreateClient();

            var content = new StringContent(message, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://localhost:7145/api/profile", content);
            return View(); 
        }

        [HttpPost("clients")]
        public async Task<IActionResult> Clients()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var message = JsonConvert.SerializeObject(new
            {
                UserId = userId
            });

            var client = _httpClientFactory.CreateClient();

            var content = new StringContent(message, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://localhost:7145/api/profile/clients", content);

            if (response.IsSuccessStatusCode)
            {
                var contentResponse = await response.Content.ReadAsStringAsync();

                var responseData = JsonConvert.DeserializeObject<dynamic>(contentResponse);

                //string userId = responseData?.UserId?.ToString();

                return Ok(new { Message = "", });
            }
            else
            {
                // Обрабатываем ошибки
                return StatusCode((int)response.StatusCode, "Error occurred while calling microservice");
            }
        }

        [HttpPost("freelancers")]
        public async Task<IActionResult> Freelancers()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var message = JsonConvert.SerializeObject(new
            {
                UserId = userId
            });

            var client = _httpClientFactory.CreateClient();

            var content = new StringContent(message, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://localhost:7145/api/profile/freelancers", content);

            if (response.IsSuccessStatusCode)
            {
                var contentResponse = await response.Content.ReadAsStringAsync();

                var responseData = JsonConvert.DeserializeObject<dynamic>(contentResponse);

                //string userId = responseData?.UserId?.ToString();

                return Ok(new { Message = "", });
            }
            else
            {
                // Обрабатываем ошибки
                return StatusCode((int)response.StatusCode, "Error occurred while calling microservice");
            }
        }
    }
}
