﻿namespace WebApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string Role { get; set; }
        public FreelancerProfile? FreelancerProfile { get; set; }
        public ClientProfile? ClientProfile { get; set; }
    }

}
