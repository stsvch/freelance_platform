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
        public void StartListeningForProjectCreated()
        {
            _rabbitMqService.ListenForMessages("ProjectCreatedQueue", async (message) =>
            {
            });
        }
    }
}
