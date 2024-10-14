using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly RabbitMqService _rabbitMqService;

        public AuthController(RabbitMqService rabbitMqService)
        {
            _rabbitMqService = rabbitMqService;
        }

        // Страница регистрации
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(UserRegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Создаем сообщение для отправки
            var message = new
            {
                Username = model.Username,
                Password = model.Password,
                Email = model.Email,
                Role = model.Role
            };

            // Отправляем сообщение в RabbitMQ
            _rabbitMqService.PublishMessage(message, "userRegistrationQueue");

            // Предполагаем, что микросервис обработает сообщение и ответит асинхронно
            return RedirectToAction("Login");
        }

        // Страница входа
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(UserLoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Создаем сообщение для отправки
            var message = new
            {
                Username = model.Username,
                Password = model.Password
            };

            // Отправляем сообщение в RabbitMQ для авторизации
            _rabbitMqService.PublishMessage(message, "userLoginQueue");

            // После отправки, редиректим на страницу профиля
            return RedirectToAction("Profile", "Profile");
        }
    }


}
