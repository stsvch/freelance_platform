using Microsoft.EntityFrameworkCore;
using RatingService.Model;

namespace RatingService.Service
{
    public class ReviewService
    {
        private readonly ReviewDbContext _context;

        public ReviewService(ReviewDbContext context)
        {
            _context = context;
        }

        public async Task<List<Review>> GetReviews(int projectId)
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
    }
}
