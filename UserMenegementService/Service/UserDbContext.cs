using Microsoft.EntityFrameworkCore;
using UserMenegementService.Model;

namespace UserMenegementService.Service
{

    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<FreelancerProfile> FreelancerProfiles { get; set; }
        public DbSet<ClientProfile> ClientProfiles { get; set; }
    }
}
