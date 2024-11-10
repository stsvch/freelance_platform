using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Newtonsoft.Json;
using NotificationService.Model;
using System.Text.Json;

namespace NotificationService.Service
{
    public class NotifyService
    {
        private readonly EmailService _emailService;
        private readonly RabbitMqService _rabbitMqService;
        private readonly Dictionary<string, string> _pendingMessages = new();

        public NotifyService(EmailService emailService, RabbitMqService rabbitMqService)
        {
            _emailService = emailService;
            _rabbitMqService = rabbitMqService;
        }

        public void StartListeningForMessages()
        {
            try
            {
                _rabbitMqService.ListenForMessages("ResponseToNotificationQueue", async message =>
                {
                    await HandleResponseMessage(message);
                });

                _rabbitMqService.ListenForMessages("UserNotificationQueue", async message =>
                {
                    await SaveMessage(message);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while starting message listeners: {ex.Message}");
            }
        }

        public async Task HandleResponseMessage(string message)
        {
            var projectMessage = JsonConvert.DeserializeObject<dynamic>(message);
            var action = projectMessage.Action.ToString();

            switch (action)
            {
                case "Accept":
                    await AcceptAsync(projectMessage);
                    break;
                case "CreateResponse":
                    await CreateResponseAsync(projectMessage);
                    break;
                default:
                    Console.WriteLine($"Неизвестное действие: {action}");
                    break;
            }
        }

        private async Task CreateResponseAsync(dynamic projectMessage)
        {
            var client = projectMessage.ClientId;
            var mail = await GetClientMail(Convert.ToInt32(client));
            await SendEmailAsync(mail, "Create");
        }

        private async Task AcceptAsync(dynamic projectMessage)
        {
            var freelancerId = projectMessage.FreelancerId;
            var mail = await GetFreelancerMail(Convert.ToInt32(freelancerId));
            await SendEmailAsync(mail, "Accept");
        }

        public async Task<string> GetAndRemoveMessage(string correlationId)
        {
            await Task.Delay(1500);
            if (_pendingMessages.TryGetValue(correlationId, out var message))
            {
                _pendingMessages.Remove(correlationId);
                using JsonDocument document = JsonDocument.Parse(message);
                string mail = document.RootElement.GetProperty("Mail").GetString();
                return mail;
            }
            return null;
        }

        public async Task SaveMessage(string message)
        {
            var response = JsonConvert.DeserializeObject<dynamic>(message);
            string correlationId = response.CorrelationId;

            _pendingMessages[correlationId] = message;
        }
        public async Task<string> GetClientMail(int clientId)
        {
            var correlationId = Guid.NewGuid().ToString();
            var message = new
            {
                Action = "GetClientMail",
                CorrelationId = correlationId,
                ClientId = clientId
            };
            await _rabbitMqService.PublishAsync("NotificationUserQueue", JsonConvert.SerializeObject(message));
            return await GetAndRemoveMessage(correlationId);
        }

        public async Task<string> GetFreelancerMail(int freelancerId)
        {
            var correlationId = Guid.NewGuid().ToString();
            var message = new
            {
                Action = "GetFreelancerMail",
                CorrelationId = correlationId,
                FreelancerId = freelancerId
            };
            await _rabbitMqService.PublishAsync("NotificationUserQueue", JsonConvert.SerializeObject(message));
            return await GetAndRemoveMessage(correlationId);
        }

        public async Task SendEmailAsync(string mail, string message)
        {
            Notification notification = new Notification()
            {
                To = mail,
                Message = message,
                Subject = "Response"
            };
            await _emailService.SendEmailAsync(notification);

        }
    }
}

