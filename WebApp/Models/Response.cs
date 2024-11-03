namespace WebApp.Models
{
    public class Response
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }  
        public int FreelancerId { get; set; }
        public int ClientId { get; set; }
        public string? Message { get; set; }
    }

}
