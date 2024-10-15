using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using WebApp.Models;
using WebApp.Services;
//var userId = HttpContext.Session.GetString("UserId");
namespace WebApp.Controllers
{
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
        public async Task<IActionResult> CreateProject([FromForm] UserLoginModel model)
        {
            var message = JsonConvert.SerializeObject(new
            {
                Action = "Login",
                Username = model.Username,
                Password = model.Password, // Пароль передаем без хеширования
            });

            var client = _httpClientFactory.CreateClient();

            // Создаем контент запроса
            var content = new StringContent(message, Encoding.UTF8, "application/json");

            // Отправляем POST-запрос к микросервису
            var response = await client.PostAsync("https://localhost:7145/api/auth/login", content);

            // Проверяем успешность ответа
            if (response.IsSuccessStatusCode)
            {
                // Читаем содержимое ответа (например, JSON с ID пользователя)
                var contentResponse = await response.Content.ReadAsStringAsync();

                // Десериализуем ответ, предполагая, что там содержится ID пользователя
                var responseData = JsonConvert.DeserializeObject<dynamic>(contentResponse);

                // Извлекаем ID пользователя из ответа
                string userId = responseData?.UserId?.ToString();

                if (!string.IsNullOrEmpty(userId))
                {
                    // Сохраняем ID пользователя в сессии
                    HttpContext.Session.SetString("UserId", userId);

                    // Либо можем сохранить ID в куках (на 1 час)
                    CookieOptions cookieOptions = new CookieOptions
                    {
                        Expires = DateTime.UtcNow.AddHours(1)
                    };
                    HttpContext.Response.Cookies.Append("UserId", userId, cookieOptions);
                }

                return Ok(new { Message = "Login successful", UserId = userId });
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
        public async Task<IActionResult> Register([FromForm] UserRegisterModel model)
        {
            var passwordHasher = new PasswordHasher<UserRegisterModel>();
            var hashedPassword = passwordHasher.HashPassword(model, model.Password);

            var message = JsonConvert.SerializeObject(new
            {
                Action = "Register",
                Username = model.Username,
                Password = hashedPassword,  
                Email = model.Email,
                Role = model.Role
            });

            var client = _httpClientFactory.CreateClient();

            // Создаем контент запроса
            var content = new StringContent(message, Encoding.UTF8, "application/json");

            // Отправляем POST-запрос к микросервису
            var response = await client.PostAsync("https://localhost:7145/api/auth/register", content);

            // Проверяем успешность ответа
            if (response.IsSuccessStatusCode)
            {
                // Читаем содержимое ответа (например, JSON с ID пользователя)
                var contentResponse = await response.Content.ReadAsStringAsync();

                // Десериализуем ответ, предполагая, что там содержится ID пользователя
                var responseData = JsonConvert.DeserializeObject<dynamic>(contentResponse);

                // Извлекаем ID пользователя из ответа
                string userId = responseData?.UserId?.ToString();

                if (!string.IsNullOrEmpty(userId))
                {
                    // Сохраняем ID пользователя в сессии
                    HttpContext.Session.SetString("UserId", userId);

                    // Либо можем сохранить ID в куках (на 1 час)
                    CookieOptions cookieOptions = new CookieOptions
                    {
                        Expires = DateTime.UtcNow.AddHours(1)
                    };
                    HttpContext.Response.Cookies.Append("UserId", userId, cookieOptions);
                }

                return Ok(new { Message = "Login successful", UserId = userId });
            }
            else
            {
                // Обрабатываем ошибки
                return StatusCode((int)response.StatusCode, "Error occurred while calling microservice");
            }
        }

    }
}
