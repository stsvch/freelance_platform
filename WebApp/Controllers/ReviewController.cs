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
        private readonly ILogger<ReviewController> _logger;

        public ReviewController(ReviewService reviewService, ILogger<ReviewController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        [HttpGet("create")]
        public IActionResult Create(int freelancerId)
        {
            try
            {
                _logger.LogInformation("Загрузка страницы для создания отзыва для фрилансера с ID {FreelancerId}", freelancerId);
                Review review = new Review();
                review.FreelancerId = freelancerId;
                return View(review);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке страницы для создания отзыва для фрилансера с ID {FreelancerId}", freelancerId);
                return StatusCode(500, "Произошла ошибка при обработке запроса.");
            }
        }

        [HttpPost("create/review")]
        public async Task<IActionResult> Create([FromForm] Review review)
        {
            try
            {
                _logger.LogInformation("Попытка создать отзыв для фрилансера с ID {FreelancerId}", review.FreelancerId);

                await _reviewService.CreateReview(review);

                var previousUrl = Request.Headers["Referer"].ToString();
                if (string.IsNullOrEmpty(previousUrl))
                {
                    _logger.LogInformation("Перенаправление на страницу профиля после создания отзыва для фрилансера с ID {FreelancerId}", review.FreelancerId);
                    return RedirectToAction("Index", "Profile");
                }

                _logger.LogInformation("Перенаправление на предыдущую страницу после создания отзыва для фрилансера с ID {FreelancerId}", review.FreelancerId);
                return Redirect(previousUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании отзыва для фрилансера с ID {FreelancerId}", review.FreelancerId);
                return StatusCode(500, "Произошла ошибка при создании отзыва.");
            }
        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("Попытка удалить отзыв с ID {ReviewId}", id);

                await _reviewService.DeleteReview(id);

                var previousUrl = Request.Headers["Referer"].ToString();
                if (string.IsNullOrEmpty(previousUrl))
                {
                    _logger.LogInformation("Перенаправление на страницу профиля после удаления отзыва с ID {ReviewId}", id);
                    return RedirectToAction("Index", "Profile");
                }

                _logger.LogInformation("Перенаправление на предыдущую страницу после удаления отзыва с ID {ReviewId}", id);
                return Redirect(previousUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении отзыва с ID {ReviewId}", id);
                return StatusCode(500, "Произошла ошибка при удалении отзыва.");
            }
        }
    }
}

