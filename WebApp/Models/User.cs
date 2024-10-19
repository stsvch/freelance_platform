namespace WebApp.Models
{
    public class UserRegister
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public string Role { get; set; } // "Freelancer" или "Client"
        public string? Skills { get; set; } // Только для фрилансера
        public string? Bio { get; set; }    // Только для фрилансера
        public string? CompanyName { get; set; } // Только для клиента
        public string? Description { get; set; } // Только для клиента
    }


    public class UserLogin
    {
        public string Email { get; set; }
        public string PasswordHash { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string Role { get; set; }

        // Связь с профилем фрилансера (nullable)
        public FreelancerProfile? FreelancerProfile { get; set; }

        // Связь с профилем клиента (nullable)
        public ClientProfile? ClientProfile { get; set; }
    }

}
