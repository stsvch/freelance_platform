using UserMenegementService.Model;
using Microsoft.EntityFrameworkCore;


namespace UserMenegementService.Service
{
    public interface IProfileService
    {
        Task<FreelancerProfile> GetFreelancerProfileAsync(int userId);
        Task<ClientProfile> GetClientProfileAsync(int userId);

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
            return await context.FreelancerProfiles.FirstOrDefaultAsync(fp => fp.UserId == userId);
        }


        public async Task<ClientProfile> GetClientProfileAsync(int userId)
        {
            return await context.ClientProfiles.FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<List<FreelancerProfile>> GetAllFreelancersExceptAsync(int userId)
        {
            return await context.FreelancerProfiles
                                 .Where(f => f.UserId != userId)
                                 .ToListAsync();
        }

        public async Task<List<ClientProfile>> GetAllClientsExceptAsync(int userId)
        {
            return await context.ClientProfiles
                                 .Where(c => c.UserId != userId)
                                 .ToListAsync();
        }
    }

}
