using Microsoft.AspNetCore.Mvc;
using NotificationService.Model;
using NotificationService.Service;

namespace NotificationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly EmailService _emailService;

        public NotificationController(EmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> SendNotification([FromBody] Notification notification)
        {
            await _emailService.SendEmailAsync(notification);
            return Ok();
        }
    }
}
