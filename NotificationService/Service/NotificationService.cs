using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using NotificationService.Model;

namespace NotificationService.Service
{
    public class NotificationService
    {
        private readonly EmailService _emailService;
        private readonly RabbitMqService _rabbitMqService;

        public NotificationService(EmailService emailService, RabbitMqService rabbitMqService)
        {
            _emailService = emailService;
            _rabbitMqService = rabbitMqService;
        }

        public async Task SendEmailAsync(Notification notification)
        {

        }
    }
}

