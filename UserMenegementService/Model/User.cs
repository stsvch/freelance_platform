using Microsoft.EntityFrameworkCore;

namespace UserMenegementService.Model
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Role { get; set; }

        // Связь с профилем фрилансера (nullable)
        public FreelancerProfile? FreelancerProfile { get; set; }

        // Связь с профилем клиента (nullable)
        public ClientProfile? ClientProfile { get; set; }
    }
}
