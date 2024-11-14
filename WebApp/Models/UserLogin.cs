using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class UserLogin
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string PasswordHash { get; set; }
    }

}
