using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserMenegementService.Model;
using UserMenegementService.Service;

namespace UserMenegementService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService userService;

        public AuthController(IUserService userService)
        {
            this.userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Model.UserRegisterModel model)
        {
            var result = await userService.RegisterUserAsync(model);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            await userService.NotifyUserRegistered(model.Email);
            return Ok("user registered successfully");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginModel model)
        {
            var token = await userService.AuthenticateAsync(model);
            if (token == null)
            {
                return Unauthorized("Invalid credentials");
            }
            return Ok(new { Token = token });
        }
    }
}
