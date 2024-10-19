namespace WebApp.Models
{
    public class FreelancerProfile
    {
        public int Id { get; set; }
        public string Skills { get; set; }
        public string Bio { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}