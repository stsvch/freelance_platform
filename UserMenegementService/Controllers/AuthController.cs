using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using UserMenegementService.Model;
using UserMenegementService.Service;


namespace UserMenegementService.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;

        public AuthController(IUserService userService, IJwtService jwtService)
        {
            _userService = userService;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginModel loginModel)
        {
            var user = await _userService.AuthenticateUserAsync(loginModel.Username, loginModel.Password);
            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid username or password" });
            }

            var token = _jwtService.GenerateToken(user);
            return Ok(new { UserId = user.Id, Username = user.Username, Token = token });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterModel registerModel)
        {
            try
            {
                var user = await _userService.RegisterUserAsync(registerModel);
                var token = _jwtService.GenerateToken(user);

                return Ok(new { UserId = user.Id, Username = user.Username, Token = token });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }

}
