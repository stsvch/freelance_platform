using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class UserRegister
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Role { get; set; } 
        public string? Skills { get; set; } 
        public string? Bio { get; set; }  
        public string? CompanyName { get; set; } 
        public string? Description { get; set; } 
    }

}
