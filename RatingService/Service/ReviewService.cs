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

        public async Task<List<Review>> GetFreelancerReviews(int freelacerId)
        {
            return await _context.Reviews
                .Where(r => r.FreelancerId == freelacerId)
                .ToListAsync();
        }

        public async Task<List<Review>> GetProjectReviews(int projectId)
        {
            return await _context.Reviews
                .Where(r => r.ProjectId == projectId)
                .ToListAsync();
        }

        public async Task<Review> CreateReview(Review review)
        {
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return review;
        }
        public async Task DeleteReview(int reviewId)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Review> UpdateReview(Review updatedReview)
        {
            var review = await _context.Reviews.FindAsync(updatedReview.Id);
            if (review == null)
            {
                return null; // Если отзыв не найден, возвращаем null
            }

            // Проверяем каждое поле на null и обновляем только если значение не null
            review.Comment = updatedReview.Comment ?? review.Comment;
            review.Rating = updatedReview.Rating ?? review.Rating;
            review.FreelancerId = updatedReview.FreelancerId != 0 ? updatedReview.FreelancerId : review.FreelancerId;
            review.ClientId = updatedReview.ClientId != 0 ? updatedReview.ClientId : review.ClientId;
            review.ProjectId = updatedReview.ProjectId != 0 ? updatedReview.ProjectId : review.ProjectId;

            // Сохраняем изменения
            await _context.SaveChangesAsync();
            return review; // Возвращаем обновленный отзыв
        }

        internal async Task<Review> GetReviewById(int reviewId)
        {
            return await _context.Reviews.FindAsync(reviewId);
        }
    }
}
