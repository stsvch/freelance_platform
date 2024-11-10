using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Controllers
{
    [ServiceFilter(typeof(RoleFilter))]
    [Route("review")]
    public class ReviewController : Controller
    {
        private readonly ReviewService _reviewService;

        public ReviewController(ReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet("create")]
        public async Task<IActionResult> Create(int freelancerId)
        {
            Review review = new Review();
            review.FreelancerId = freelancerId;
            return View(review);
        }

        [HttpPost("create/review")]
        public async Task<IActionResult> Create([FromForm] Review review)
        {
            var previousUrl = Request.Headers["Referer"].ToString();
            await _reviewService.CreateReview(review);
            if (string.IsNullOrEmpty(previousUrl))
            {
                return RedirectToAction("Index", "Profile");
            }

            return Redirect(previousUrl);
        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var previousUrl = Request.Headers["Referer"].ToString();
            await _reviewService.DeleteReview(id);
            if (string.IsNullOrEmpty(previousUrl))
            {
                return RedirectToAction("Index", "Profile");
            }

            return Redirect(previousUrl);
        }
    }
}
