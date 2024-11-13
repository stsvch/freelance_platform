using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UserMenegementService.Service;

namespace UserMenegementService.Controllers
{
    [ApiController]
    [Route("api/profile")]
    public class UserProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;
        private readonly ILogger<UserProfileController> _logger;

        public UserProfileController(IProfileService profileService, ILogger<UserProfileController> logger)
        {
            _profileService = profileService;
            _logger = logger;
        }

        [HttpGet("freelancer/{userId}")]
        public async Task<IActionResult> GetFreelancerProfile(int userId)
        {
            try
            {
                _logger.LogInformation("Запрос на получение профиля фрилансера с ID: {UserId}", userId);

                var profile = await _profileService.GetFreelancerProfileAsync(userId);
                if (profile == null)
                {
                    _logger.LogWarning("Профиль фрилансера с ID {UserId} не найден.", userId);
                    return NotFound("Freelancer profile not found.");
                }

                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении профиля фрилансера с ID {UserId}", userId);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("client/{userId}")]
        public async Task<IActionResult> GetClientProfile(int userId)
        {
            try
            {
                _logger.LogInformation("Запрос на получение профиля клиента с ID: {UserId}", userId);

                var profile = await _profileService.GetClientProfileAsync(userId);
                if (profile == null)
                {
                    _logger.LogWarning("Профиль клиента с ID {UserId} не найден.", userId);
                    return NotFound("Client profile not found.");
                }

                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении профиля клиента с ID {UserId}", userId);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("freelancers")]
        public async Task<IActionResult> GetFreelancers([FromQuery] string userId)
        {
            try
            {
                _logger.LogInformation("Запрос на получение списка фрилансеров, исключая пользователя с ID {UserId}", userId);

                var freelancers = await _profileService.GetAllFreelancersExceptAsync(int.Parse(userId));
                return Ok(freelancers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка фрилансеров.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("clients")]
        public async Task<IActionResult> GetClients([FromQuery] string userId)
        {
            try
            {
                _logger.LogInformation("Запрос на получение списка клиентов, исключая пользователя с ID {UserId}", userId);

                var clients = await _profileService.GetAllClientsExceptAsync(int.Parse(userId));
                return Ok(clients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка клиентов.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("freelancerId/{id}")]
        public async Task<IActionResult> GetFreelancerProfileById(int id)
        {
            try
            {
                _logger.LogInformation("Получение профиля фрилансера по ID: {Id}", id);

                var profile = await _profileService.GetFreelancerProfileByIdAsync(id);
                if (profile == null)
                {
                    _logger.LogWarning("Профиль фрилансера с ID {Id} не найден.", id);
                    return NotFound("Freelancer profile not found.");
                }

                _logger.LogInformation("Профиль фрилансера с ID {Id} успешно получен.", id);
                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении профиля фрилансера с ID {Id}", id);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("clientId/{id}")]
        public async Task<IActionResult> GetClientProfileById(int id)
        {
            try
            {
                _logger.LogInformation("Получение профиля клиента по ID: {Id}", id);

                var profile = await _profileService.GetClientProfileByIdAsync(id);
                if (profile == null)
                {
                    _logger.LogWarning("Профиль клиента с ID {Id} не найден.", id);
                    return NotFound("Client profile not found.");
                }

                _logger.LogInformation("Профиль клиента с ID {Id} успешно получен.", id);
                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении профиля клиента с ID {Id}", id);
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}

