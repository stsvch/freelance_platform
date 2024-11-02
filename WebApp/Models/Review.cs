namespace WebApp.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }  // Ссылка на проект
        public int ClientId { get; set; }
        public int FreelancerId { get; set; }
        public string? Comment { get; set; }
        public int? Rating { get; set; }      // Рейтинг, например, от 1 до 5
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
