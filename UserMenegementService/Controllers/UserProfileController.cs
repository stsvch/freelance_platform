using Microsoft.AspNetCore.Mvc;
using UserMenegementService.Model;
using UserMenegementService.Service;

namespace UserMenegementService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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

        [HttpPost("freelancer")]
        public async Task<IActionResult> CreateFreelancerProfile([FromBody] FreelancerProfile profile)
        {
            await _profileService.CreateFreelancerProfileAsync(profile);
            return CreatedAtAction(nameof(GetFreelancerProfile), new { userId = profile.UserId }, profile);
        }
    }
}
