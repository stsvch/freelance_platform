using Microsoft.AspNetCore.Mvc;
using UserMenegementService.Model;
using UserMenegementService.Service;

namespace UserMenegementService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public UserProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet("freelancer/{userId}")]
        public async Task<IActionResult> GetFreelancerProfile(int userId)
        {
            var profile = await _profileService.GetFreelancerProfileAsync(userId);
            if (profile == null)
            {
                return NotFound("Freelancer profile not found.");
            }
            return Ok(profile);
        }

        [HttpGet("client/{userId}")]
        public async Task<IActionResult> GetClientProfile(int userId)
        {
            var profile = await _profileService.GetFreelancerProfileAsync(userId);
            if (profile == null)
            {
                return NotFound("Client profile not found.");
            }
            return Ok(profile);
        }

        [HttpPost("freelancers")]
        public async Task<IActionResult> GetFreelancers([FromBody] User model)
        {
            try
            {
                var freelancers = await _profileService.GetAllFreelancersExceptAsync(model.Id);
                return Ok(freelancers);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("clients")]
        public async Task<IActionResult> GetClients([FromBody] User model)
        {
            try
            {
                var clients = await _profileService.GetAllClientsExceptAsync(model.Id);
                return Ok(clients);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

    }
}
