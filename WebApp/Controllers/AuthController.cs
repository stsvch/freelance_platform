using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Страница регистрации
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(UserRegisterModel model)
        {
            var client = _httpClientFactory.CreateClient("UserManagementService");
            var response = await client.PostAsJsonAsync("auth/register", model);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Login");
            }

            ModelState.AddModelError("", "Ошибка регистрации.");
            return View(model);
        }

        // Страница входа
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(UserLoginModel model)
        {
            var client = _httpClientFactory.CreateClient("UserManagementService");
            var response = await client.PostAsJsonAsync("auth/login", model);

            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadAsStringAsync();
                // Логика сохранения токена
                return RedirectToAction("Profile", "Profile");
            }

            ModelState.AddModelError("", "Неверные учетные данные.");
            return View(model);
        }
    }

}
