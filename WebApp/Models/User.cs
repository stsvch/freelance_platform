namespace WebApp.Models
{
    public class ProjectModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Budget { get; set; }
    }

    public class UserRegisterModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Role { get; set; } 
    }

    public class UserLoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
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
