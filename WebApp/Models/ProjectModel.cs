namespace WebApp.Models
{
    public class ProjectModel
    {
        public int? Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string? Status { get; set; }
        public int Budget { get; set; }
        public int ClientId { get; set; } // Ссылка на заказчика
        public int? FreelancerId { get; set; } // Ссылка на фрилансера
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
