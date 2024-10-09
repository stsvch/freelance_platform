using UserMenegementService.Model;
using Microsoft.EntityFrameworkCore;


namespace UserMenegementService.Service
{
    public interface IProfileService
    {
        Task<FreelancerProfile> GetFreelancerProfileAsync(int userId);
        Task CreateFreelancerProfileAsync(FreelancerProfile profile);
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

        public async Task CreateFreelancerProfileAsync(FreelancerProfile profile)
        {
            context.FreelancerProfiles.Add(profile);
            await context.SaveChangesAsync();
        }
    }

}
