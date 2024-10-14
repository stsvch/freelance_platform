using Microsoft.EntityFrameworkCore;
using RatingService.Model;

namespace RatingService.Service
{
    public class ReviewService
    {
        private readonly ReviewDbContext _context;
        private readonly RabbitMqService _rabbitMqService;

        public ReviewService(ReviewDbContext context, RabbitMqService rabbitMqService)
        {
            _context = context;
            _rabbitMqService = rabbitMqService;
        }

        // Получить все отзывы для проекта
        public async Task<List<Review>> GetReviews(int projectId)
        {
            return await _context.Reviews
                .Where(r => r.ProjectId == projectId)
                .ToListAsync();
        }

        // Создать новый отзыв
        public async Task<Review> CreateReview(Review review)
        {
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Отправить сообщение о новом отзыве в RabbitMQ
            _rabbitMqService.Publish($"New review created for project {review.ProjectId}", "ReviewCreatedQueue");

            return review;
        }

        // Удалить отзыв
        public async Task DeleteReview(int reviewId)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
            }
        }

        // Начать прослушивание сообщений для создания отзывов, получаем события о проектах
        public void StartListeningForProjectCreated()
        {
            _rabbitMqService.ListenForMessages("ProjectCreatedQueue", async (message) =>
            {
                // Пример простого создания отзыва для проекта
                var projectId = int.Parse(message);  // Преобразуем сообщение в ID проекта (можно добавить более сложную логику)
                var newReview = new Review
                {
                    ProjectId = projectId,
                    UserId = 1,  // Здесь можно добавить логику получения текущего пользователя
                    Comment = "Great project!",
                    Rating = 5
                };

                await CreateReview(newReview);
            });
        }
    }
}
