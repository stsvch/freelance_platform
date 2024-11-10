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

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLogin loginModel)
        {
            var user = await _userService.AuthenticateUserAsync(loginModel.Email, loginModel.PasswordHash);
            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid username or password" });
            }
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

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegister registerModel)
        {
            try
            {
                var user = await _userService.RegisterUserAsync(registerModel);
                int id;
               if(user.Role == "Client")
                {
                    id = user.ClientProfile.Id;
                }
                else
                {
                    id = user.FreelancerProfile.Id;
                }
                return Ok(new { UserId = user.Id, Username = user.Username,  Role = user.Role, Id = id });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

    }

}
