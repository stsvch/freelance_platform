using Microsoft.AspNetCore.Mvc;
using UserMenegementService.Model;
using UserMenegementService.Service;

namespace UserMenegementService.Controllers
{
    [ApiController]
    [Route("api/profile")]
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
            var profile = await _profileService.GetClientProfileAsync(userId);
            if (profile == null)
            {
                return NotFound("Client profile not found.");
            }
            return Ok(profile);
        }

        [HttpGet("freelancerId/{id}")]
        public async Task<IActionResult> GetFreelancerProfileById(int id)
        {
            var profile = await _profileService.GetFreelancerProfileByIdAsync(id);
            if (profile == null)
            {
                return NotFound("Freelancer profile not found.");
            }
            return Ok(profile);
        }

        [HttpGet("clientId/{id}")]
        public async Task<IActionResult> GetClientProfileById(int id)
        {
            var profile = await _profileService.GetClientProfileByIdAsync(id);
            if (profile == null)
            {
                return NotFound("Client profile not found.");
            }
            return Ok(profile);
        }

        [HttpGet("freelancers")]
        public async Task<IActionResult> GetFreelancers([FromQuery] string userId)
        {
            try
            {
                // Получаем список всех фрилансеров, кроме текущего пользователя
                var freelancers = await _profileService.GetAllFreelancersExceptAsync(int.Parse(userId));
                return Ok(freelancers);  // Возвращаем список фрилансеров
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }


        [HttpGet("clients")]
        public async Task<IActionResult> GetClients([FromQuery] string userId)
        {
            try
            {
                var clients = await _profileService.GetAllClientsExceptAsync(int.Parse(userId));
                return Ok(clients);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

    }
}
