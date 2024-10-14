namespace RatingService.Model
{
    public class Review
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }  // Ссылка на проект
        public int UserId { get; set; }     // Ссылка на пользователя (клиент или фрилансер)
        public string Comment { get; set; }
        public int Rating { get; set; }      // Рейтинг, например, от 1 до 5
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
