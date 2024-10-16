namespace UserMenegementService.Model
{
    public class UserRegisterModel
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


}
