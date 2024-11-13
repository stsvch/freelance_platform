using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Controllers
{
    [ServiceFilter(typeof(RoleFilter))]
    [Route("auth")]
    public class AuthController : Controller
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            _logger.LogInformation("Пользователь вышел из системы.");
            return RedirectToAction("Login");
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLogin model)
        {
            try
            {
                var response = await _authService.Login(model);
                if (response != null)
                {
                    string userId = response?.userId?.ToString();
                    string role = response?.role?.ToString();
                    string id = response?.id?.ToString();

                    if (!string.IsNullOrEmpty(userId))
                    {
                        HttpContext.Session.SetString("UserId", userId);
                        HttpContext.Session.SetString("Role", role);
                        HttpContext.Session.SetString("Id", id);

                        _logger.LogInformation("Успешный вход пользователя с ID: {UserId}, роль: {Role}", userId, role);
                    }

                    return RedirectToAction("Index", "Profile");
                }
                else
                {
                    _logger.LogWarning("Ошибка входа: неверное имя пользователя или пароль.");
                    ViewBag.ErrorMessage = "Неверное имя пользователя или пароль. Попробуйте снова.";
                    return View("Login");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неожиданная ошибка при входе.");
                ViewBag.ErrorMessage = "Произошла ошибка при попытке входа. Попробуйте позже.";
                return View("Login");
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
            try
            {
                var response = await _authService.Register(model);
                if (response != null)
                {
                    string userId = response?.userId?.ToString();
                    string role = response?.role?.ToString();
                    string id = response?.id?.ToString();

                    if (!string.IsNullOrEmpty(userId))
                    {
                        HttpContext.Session.SetString("UserId", userId);
                        HttpContext.Session.SetString("Role", role);
                        HttpContext.Session.SetString("Id", id);

                        _logger.LogInformation("Успешная регистрация пользователя с ID: {UserId}, роль: {Role}", userId, role);
                    }

                    return RedirectToAction("Index", "Profile");
                }
                else
                {
                    _logger.LogWarning("Ошибка регистрации: регистрация не удалась.");
                    ViewBag.ErrorMessage = "Не удалось зарегистрировать пользователя. Попробуйте снова.";
                    return View("Register");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неожиданная ошибка при регистрации.");
                ViewBag.ErrorMessage = "Произошла ошибка при попытке регистрации. Попробуйте позже.";
                return View("Register");
            }
        }
    }
}

