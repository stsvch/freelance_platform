using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Runtime.InteropServices.Marshalling;
using UserMenegementService.Model;
using UserMenegementService.Service;


namespace UserMenegementService.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, ILogger<AuthController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLogin loginModel)
        {
            try
            {
                var user = await _userService.AuthenticateUserAsync(loginModel.Email, loginModel.PasswordHash);

                if (user == null)
                {
                    _logger.LogWarning("Неудачная попытка входа для пользователя с email {Email}. Неверный логин или пароль.", loginModel.Email);
                    return Unauthorized(new { Message = "Invalid username or password" });
                }
                _logger.LogInformation("Пользователь с email {Email} успешно аутентифицирован.", loginModel.Email);
                int id;
                if (user.Role == "Client")
                {
                    id = user.ClientProfile.Id;
                }
                else
                {
                    id = user.FreelancerProfile.Id;
                }
                return Ok(new { UserId = user.Id, Role = user.Role, Id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при попытке входа для пользователя с email {Email}", loginModel.Email);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegister registerModel)
        {
            try
            {
                var user = await _userService.RegisterUserAsync(registerModel);
                int id;
                if (user.Role == "Client")
                {
                    id = user.ClientProfile.Id;
                }
                else
                {
                    id = user.FreelancerProfile.Id;
                }

                _logger.LogInformation("Пользователь с email {Email} успешно зарегистрирован с ролью {Role}.", registerModel.Email, user.Role);

                return Ok(new { UserId = user.Id, Username = user.Username, Role = user.Role, Id = id });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Ошибка при регистрации пользователя с email {Email}.", registerModel.Email);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неизвестная ошибка при регистрации пользователя с email {Email}.", registerModel.Email);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }
    }
}

