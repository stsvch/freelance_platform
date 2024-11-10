namespace RatingService.Model
{
    public class Review
    {
        public int Id { get; set; }
        public int ProjectId { get; set; } 
        public int ClientId { get; set; }
        public int FreelancerId { get; set; }
        public string? Comment { get; set; }
        public int? Rating { get; set; }      
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
