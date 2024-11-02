namespace RatingService.Model
{
    public class Response
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }  // Ссылка на проект
        public int FreelancerId { get; set; }
        public int ClientId {  get; set; }
    }
}
