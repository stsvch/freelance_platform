using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;
using System.Text;
using WebApp.Models;
using WebApp.Services;
//var userId = HttpContext.Session.GetString("UserId");
namespace WebApp.Controllers
{
    [ServiceFilter(typeof(RoleFilter))]
    [Route("auth")]
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLogin model)
        {
            // Хешируем пароль с использованием метода HashPassword
            var passwordHash = HashPassword.Hash(model.PasswordHash);

            // Создаем объект для отправки на микросервис
            var loginRequest = new
            {
                Email = model.Email,
                PasswordHash = passwordHash
            };

            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(JsonConvert.SerializeObject(loginRequest), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://localhost:7145/api/auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                var contentResponse = await response.Content.ReadAsStringAsync();
                var responseData = JsonConvert.DeserializeObject<dynamic>(contentResponse);

                string userId = responseData?.userId?.ToString();
                string role = responseData?.role?.ToString();
                string id = responseData?.id?.ToString();


                if (!string.IsNullOrEmpty(userId))
                {
                    // Сохраняем ID пользователя и роль в сессии
                    HttpContext.Session.SetString("UserId", userId);
                    HttpContext.Session.SetString("Role", role);
                    HttpContext.Session.SetString("Id", id);
                }

                return RedirectToAction("Index", "Profile");
            }
            else
            {
                // Обрабатываем ошибки
                return StatusCode((int)response.StatusCode, "Error occurred while calling microservice");
            }
        }


        [HttpGet("register")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegister model)
        {
            string message;
            if(model.Role=="Client")
            {
                message = JsonConvert.SerializeObject(new
                {
                    Username = model.Username,
                    PasswordHash = HashPassword.Hash(model.PasswordHash),  // Пароль передается без хеширования
                    Email = model.Email,
                    Role = model.Role,
                    Description = model.Description,
                    CompanyName = model.CompanyName
                });
            }
            else 
            {
                message = JsonConvert.SerializeObject(new
                {
                    Username = model.Username,
                    PasswordHash = HashPassword.Hash(model.PasswordHash),  // Пароль передается без хеширования
                    Email = model.Email,
                    Role = model.Role,
                    Skills = model.Skills,
                    Bio = model.Bio
                });
            }


            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(message, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://localhost:7145/api/auth/register", content);

            if (response.IsSuccessStatusCode)
            {
                var contentResponse = await response.Content.ReadAsStringAsync();
                var responseData = JsonConvert.DeserializeObject<dynamic>(contentResponse);

                // Извлекаем ID пользователя из ответа
                string userId = responseData?.userId?.ToString();
                string role = responseData?.role?.ToString();
                string id = responseData?.id?.ToString();

                if (!string.IsNullOrEmpty(userId))
                {
                    // Сохраняем ID пользователя и роль в сессии
                    HttpContext.Session.SetString("UserId", userId);
                    HttpContext.Session.SetString("Role", role);
                    HttpContext.Session.SetString("Id", id);
                }

                return RedirectToAction("Index", "Profile");
            }
            else
            {
                // Обрабатываем ошибки
                return StatusCode((int)response.StatusCode, "Error occurred while calling microservice");
            }
        }
    }
}
