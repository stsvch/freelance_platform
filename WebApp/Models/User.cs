namespace WebApp.Models
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


    public class UserLoginModel
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
    }

    public class FreelancerProfileModel
    {
        public string Skills { get; set; }
        public string Bio { get; set; }
        public List<ProjectModel> CurrentProjects { get; set; } = new List<ProjectModel>();
        public List<ProjectModel> CompletedProjects { get; set; } = new List<ProjectModel>();
    }

    public class ClientProfileModel
    {
        public string CompanyName { get; set; }
        public string Description { get; set; }
        public List<ProjectModel> CreatedProjects { get; set; } = new List<ProjectModel>();
    }


}
