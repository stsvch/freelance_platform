using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Controllers
{
    [ServiceFilter(typeof(RoleFilter))]
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
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = _httpClientFactory.CreateClient();
            HttpResponseMessage response = null;
            object profileData = null; 

            switch (role)
            {
                case "Freelancer":
                    {
                        response = await client.GetAsync($"https://localhost:7145/api/profile/freelancer/{userId}");
                        break;
                    }
                case "Client":
                    {
                        response = await client.GetAsync($"https://localhost:7145/api/profile/client/{userId}");
                        break;
                    }
                default:
                    return RedirectToAction("Login", "Auth");
            }
            if (response.IsSuccessStatusCode)
            {
                var contentResponse = await response.Content.ReadAsStringAsync();
                if (role == "Freelancer")
                {
                    profileData = JsonConvert.DeserializeObject<FreelancerProfile>(contentResponse);
                }
                else if (role == "Client")
                {
                    profileData = JsonConvert.DeserializeObject<ClientProfile>(contentResponse);
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Не удалось загрузить профиль. Попробуйте позже.";
                return View();
            }
            return View(profileData);
        }

        [HttpGet("freelancer")]
        public async Task<IActionResult> Freelancer(int id)
        {
            object profileData = null;
            var client = _httpClientFactory.CreateClient();
            HttpResponseMessage response = null;
            if (id!=null)
            {
                response = await client.GetAsync($"https://localhost:7145/api/profile/freelancer/{id}");
                var contentResponse = await response.Content.ReadAsStringAsync();
                profileData = JsonConvert.DeserializeObject<FreelancerProfile>(contentResponse);

            }
            else
            {
                ViewBag.ErrorMessage = "Не удалось загрузить профиль. Попробуйте позже.";
                return View("Index");
            }
            return View("Index", profileData);
        }

        [HttpGet("client")]
        public async Task<IActionResult> Client(int id)
        {
            object profileData = null;
            var client = _httpClientFactory.CreateClient();
            HttpResponseMessage response = null;
            if (id != null)
            {
                response = await client.GetAsync($"https://localhost:7145/api/profile/client/{id}");
                var contentResponse = await response.Content.ReadAsStringAsync();
                profileData = JsonConvert.DeserializeObject<ClientProfile>(contentResponse);
            }
            else
            {
                ViewBag.ErrorMessage = "Не удалось загрузить профиль. Попробуйте позже.";
                return View("Index");
            }
            return View("Index", profileData);
        }

        [HttpGet("clients")]
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
                var responseData = JsonConvert.DeserializeObject<List<ClientProfile>>(contentResponse);
                return View(responseData);  // Передаем список клиентов в представление
            }
            else
            {
                ViewBag.ErrorMessage = "Не удалось загрузить список клиентов. Попробуйте позже.";
                return View();  // Если не удалось загрузить данные
            }
        }

        [HttpGet("freelancers")]
        public async Task<IActionResult> Freelancers()
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                ViewBag.ErrorMessage = "Не удалось получить UserId из сессии.";
                return View();
            }

            var client = _httpClientFactory.CreateClient();

            // Формируем URL с параметром userId, который будет передан в запрос
            var url = $"https://localhost:7145/api/profile/freelancers?userId={userId}";

            var response = await client.GetAsync(url);  // Отправляем GET-запрос

            if (response.IsSuccessStatusCode)
            {
                var contentResponse = await response.Content.ReadAsStringAsync();
                var responseData = JsonConvert.DeserializeObject<List<FreelancerProfile>>(contentResponse);
                return View(responseData);  // Передаем список фрилансеров в представление
            }
            else
            {
                ViewBag.ErrorMessage = "Не удалось загрузить список фрилансеров. Попробуйте позже.";
                return View();  // Если не удалось загрузить данные
            }
        }


    }
}
