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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка связи "один к одному" между User и FreelancerProfile
            modelBuilder.Entity<FreelancerProfile>()
                .HasOne(fp => fp.User)
                .WithOne(u => u.FreelancerProfile)
                .HasForeignKey<FreelancerProfile>(fp => fp.UserId);

            // Настройка связи "один к одному" между User и ClientProfile
            modelBuilder.Entity<ClientProfile>()
                .HasOne(cp => cp.User)
                .WithOne(u => u.ClientProfile)
                .HasForeignKey<ClientProfile>(cp => cp.UserId);
        }
    }

}
