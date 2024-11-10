using UserMenegementService.Model;
using Microsoft.EntityFrameworkCore;


namespace UserMenegementService.Service
{
    public interface IProfileService
    {
        Task<FreelancerProfile> GetFreelancerProfileAsync(int userId);
        Task<ClientProfile> GetClientProfileAsync(int userId);

        Task<FreelancerProfile> GetFreelancerProfileByIdAsync(int id);  // Поиск по Id профиля
        Task<ClientProfile> GetClientProfileByIdAsync(int id);

        Task<List<FreelancerProfile>> GetAllFreelancersExceptAsync(int userId);
        Task<List<ClientProfile>> GetAllClientsExceptAsync(int userId);
    }

    public class ProfileService : IProfileService
    {
        private readonly UserDbContext context;

        public ProfileService(UserDbContext context)
        {
            this.context = context;
        }
        public async Task<FreelancerProfile> GetFreelancerProfileAsync(int userId)
        {
            return await context.FreelancerProfiles
                     .Include(fp => fp.User) 
                     .FirstOrDefaultAsync(fp => fp.UserId == userId);
        }


        public async Task<ClientProfile> GetClientProfileAsync(int userId)
        {
            return await context.ClientProfiles
                     .Include(fp => fp.User) 
                     .FirstOrDefaultAsync(fp => fp.UserId == userId);
        }

        public async Task<List<FreelancerProfile>> GetAllFreelancersExceptAsync(int userId)
        {
            return await context.FreelancerProfiles
                                 .Where(f => f.UserId != userId)
                                 .Include(f => f.User)
                                 .ToListAsync();
        }

        public async Task<List<ClientProfile>> GetAllClientsExceptAsync(int userId)
        {
            return await context.ClientProfiles
                                 .Where(c => c.UserId != userId)
                                 .Include(с => с.User)
                                 .ToListAsync();
        }

        public async Task<FreelancerProfile> GetFreelancerProfileByIdAsync(int id)
        {
            return await context.FreelancerProfiles
                .Include(fp => fp.User)
                .FirstOrDefaultAsync(fp => fp.Id == id);  // Поиск по Id профиля
        }

        public async Task<ClientProfile> GetClientProfileByIdAsync(int id)
        {
            return await context.ClientProfiles
                .Include(cp => cp.User)
                .FirstOrDefaultAsync(cp => cp.Id == id);
        }
    }

}
