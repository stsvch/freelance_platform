namespace SearchService.Model
{
    public class Project
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; } // Например: "Open", "InProgress", "Completed"
        public int UserId { get; set; } // Связь с пользователем
    }
}
