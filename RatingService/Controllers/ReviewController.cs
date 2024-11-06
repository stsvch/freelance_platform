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

        [HttpGet("freelancer/{freelancerId}")]
        public async Task<IActionResult> GetFreelancerReviews(int freelancerId)
        {
            if (freelancerId <= 0)
            {
                return BadRequest("Invalid freelancer ID.");
            }

            try
            {
                var reviews = await _reviewService.GetFreelancerReviews(freelancerId);
                if (reviews == null || !reviews.Any())
                {
                    return NotFound("No reviews found for the specified freelancer.");
                }

                return Ok(reviews);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving reviews.");
            }
        }

        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetProjectReviews(int projectId)
        {
            if (projectId <= 0)
            {
                return BadRequest("Invalid project ID.");
            }

            try
            {
                var reviews = await _reviewService.GetProjectReviews(projectId);
                if (reviews == null || !reviews.Any())
                {
                    return NotFound("No reviews found for the specified project.");
                }

                return Ok(reviews);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving project reviews.");
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateReview([FromBody] Review review)
        {
            if (review == null)
            {
                return BadRequest("Review data is required.");
            }

            try
            {
                var createdReview = await _reviewService.CreateReview(review);
                return CreatedAtAction(nameof(GetFreelancerReviews), new { freelancerId = createdReview.FreelancerId }, createdReview);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while creating the review.");
            }
        }

        [HttpPost("delete/{reviewId}")]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            if (reviewId <= 0)
            {
                return BadRequest("Invalid review ID.");
            }

            try
            {
                var review = await _reviewService.GetReviewById(reviewId);
                if (review == null)
                {
                    return NotFound("Review not found.");
                }

                await _reviewService.DeleteReview(reviewId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while deleting the review.");
            }
        }


        [HttpPost("update")]
        public async Task<IActionResult> ChangeReview([FromBody] Review review)
        {
            if (review == null || review.Id == 0)
            {
                return BadRequest("Invalid review data.");
            }

            var updatedReview = await _reviewService.UpdateReview(review);
            if (updatedReview == null)
            {
                return NotFound("Review not found.");
            }

            return NoContent();
        }

    }
}
