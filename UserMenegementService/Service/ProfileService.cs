using UserMenegementService.Model;
using Microsoft.EntityFrameworkCore;


namespace UserMenegementService.Service
{
    public interface IProfileService
    {
        Task<FreelancerProfile> GetFreelancerProfileAsync(int userId);
        Task<ClientProfile> GetClientProfileAsync(int userId);

        Task<FreelancerProfile> GetFreelancerProfileByIdAsync(int id); 
        Task<ClientProfile> GetClientProfileByIdAsync(int id);

        Task<List<FreelancerProfile>> GetAllFreelancersExceptAsync(int userId);
        Task<List<ClientProfile>> GetAllClientsExceptAsync(int userId);
    }

    public class ProfileService : IProfileService
    {
        private readonly UserDbContext _context;
        private readonly ILogger<ProfileService> _logger;
        public ProfileService(UserDbContext context, ILogger<ProfileService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<FreelancerProfile> GetFreelancerProfileAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Запрос профиля фрилансера с ID: {UserId}", userId);

                var profile = await _context.FreelancerProfiles
                                             .Include(fp => fp.User)
                                             .FirstOrDefaultAsync(fp => fp.UserId == userId);

                if (profile == null)
                {
                    _logger.LogWarning("Профиль фрилансера с ID {UserId} не найден.", userId);
                }
                else
                {
                    _logger.LogInformation("Профиль фрилансера с ID {UserId} успешно найден.", userId);
                }

                return profile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении профиля фрилансера с ID {UserId}", userId);
                throw new InvalidOperationException("Ошибка при получении профиля фрилансера", ex); 
            }
        }
        public async Task<ClientProfile> GetClientProfileAsync(int userId)
        {
            try
            {
                var profile = await _context.ClientProfiles
                                             .Include(fp => fp.User)
                                             .FirstOrDefaultAsync(fp => fp.UserId == userId);
                if (profile == null)
                {
                    _logger.LogWarning("Профиль клиента с ID {UserId} не найден.", userId);
                }
                else
                {
                    _logger.LogInformation("Профиль клиента с ID {UserId} успешно найден.", userId);
                }
                return profile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении профиля клиента с ID {UserId}", userId);
                throw new InvalidOperationException("Ошибка при получении профиля клиента", ex);
            }
        }
        public async Task<List<FreelancerProfile>> GetAllFreelancersExceptAsync(int userId)
        {
            try
            {
                var freelancers = await _context.FreelancerProfiles
                                                 .Where(f => f.UserId != userId)
                                                 .Include(f => f.User)
                                                 .ToListAsync();
                if (freelancers.Any())
                {
                    _logger.LogInformation("Найдено {FreelancerCount} фрилансеров, исключая пользователя с ID {UserId}.", freelancers.Count, userId);
                }
                else
                {
                    _logger.LogWarning("Не найдено фрилансеров, исключая пользователя с ID {UserId}.", userId);
                }

                return freelancers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка фрилансеров, исключая пользователя с ID {UserId}.", userId);
                throw new InvalidOperationException("Ошибка при получении списка фрилансеров", ex);
            }
        }

        public async Task<List<ClientProfile>> GetAllClientsExceptAsync(int userId)
        {
            try
            {
                var clients = await _context.ClientProfiles
                                             .Where(c => c.UserId != userId)
                                             .Include(c => c.User)
                                             .ToListAsync();

                if (clients.Any())
                {
                    _logger.LogInformation("Найдено {ClientCount} клиентов, исключая пользователя с ID {UserId}.", clients.Count, userId);
                }
                else
                {
                    _logger.LogWarning("Не найдено клиентов, исключая пользователя с ID {UserId}.", userId);
                }

                return clients;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка клиентов, исключая пользователя с ID {UserId}.", userId);
                throw new InvalidOperationException("Ошибка при получении списка клиентов", ex);
            }
        }
        public async Task<FreelancerProfile> GetFreelancerProfileByIdAsync(int id)
        {
            try
            {
                var freelancerProfile = await _context.FreelancerProfiles
                                                       .Include(fp => fp.User)
                                                       .FirstOrDefaultAsync(fp => fp.Id == id);

                if (freelancerProfile != null)
                {
                    _logger.LogInformation("Профиль фрилансера с ID {FreelancerId} найден.", id);
                }
                else
                {
                    _logger.LogWarning("Профиль фрилансера с ID {FreelancerId} не найден.", id);
                }

                return freelancerProfile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении профиля фрилансера с ID {FreelancerId}", id);
                throw new InvalidOperationException("Ошибка при получении профиля фрилансера", ex);
            }
        }
        public async Task<ClientProfile> GetClientProfileByIdAsync(int id)
        {
            try
            {
                var clientProfile = await _context.ClientProfiles
                                                  .Include(cp => cp.User)
                                                  .FirstOrDefaultAsync(cp => cp.Id == id);
                if (clientProfile != null)
                {
                    _logger.LogInformation("Профиль клиента с ID {ClientId} найден.", id);
                }
                else
                {
                    _logger.LogWarning("Профиль клиента с ID {ClientId} не найден.", id);
                }

                return clientProfile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении профиля клиента с ID {ClientId}", id);
                throw new InvalidOperationException("Ошибка при получении профиля клиента", ex); 
            }
        }
    }

}
