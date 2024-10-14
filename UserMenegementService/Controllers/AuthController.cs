using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using UserMenegementService.Model;
using UserMenegementService.Service;


namespace UserMenegementService.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly RabbitMqService _rabbitMqService;

        public AuthController(IUserService userService, RabbitMqService rabbitMqService)
        {
            _userService = userService;
            _rabbitMqService = rabbitMqService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterModel model)
        {
            var result = await _userService.RegisterUserAsync(model);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // Отправляем сообщение в очередь RabbitMQ
            await _rabbitMqService.PublishMessageAsync(new { Email = model.Email }, "UserRegisteredQueue");

            return Ok("User registered successfully.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginModel model)
        {
            var token = await _userService.AuthenticateAsync(model);
            if (token == null)
            {
                return Unauthorized("Invalid credentials.");
            }

            // Отправляем запрос на аутентификацию в RabbitMQ
            await _rabbitMqService.PublishMessageAsync(new { Username = model.Username }, "UserLoginQueue");

            return Ok(new { Token = token });
        }
    }

}
