using Microsoft.AspNetCore.Mvc;
using RatingService.Model;
using RatingService.Service;

namespace RatingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly ReviewService _reviewService;

        public ReviewController(ReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet("{freelancerId}")]
        public async Task<IActionResult> GetReviews(int freelancerId)
        {
            var reviews = await _reviewService.GetFreelancerReviews(freelancerId);
            return Ok(reviews);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] Review review)
        {
            var createdReview = await _reviewService.CreateReview(review);
            return Ok(review);
        }

        [HttpPost("delete/{reviewId}")]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            await _reviewService.DeleteReview(reviewId);
            return NoContent();
        }
    }
}
