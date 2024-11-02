namespace WebApp.Models
{
    public class Response
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }  // Ссылка на проект
        public int FreelancerId { get; set; }
    }

}
