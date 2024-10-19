using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

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

        [JsonIgnore]
        public FreelancerProfile? FreelancerProfile { get; set; }

        [JsonIgnore]
        public ClientProfile? ClientProfile { get; set; }
    }
}
