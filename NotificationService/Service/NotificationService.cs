using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Newtonsoft.Json;
using NotificationService.Model;

namespace NotificationService.Service
{
    public class NotificationService
    {
        private readonly EmailService _emailService;
        private readonly RabbitMqService _rabbitMqService;
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, string> _pendingMessages = new();

        public NotificationService(EmailService emailService, RabbitMqService rabbitMqService, IServiceProvider serviceProvider)
        {
            _emailService = emailService;
            _rabbitMqService = rabbitMqService;
            _serviceProvider = serviceProvider;
        }

        public void StartListeningForMessages()
        {
            _rabbitMqService.ListenForMessages("ResponseToNotificationQueue", async message =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var projectService = scope.ServiceProvider.GetRequiredService<NotificationService>();
                    await projectService.HandleResponseMessage(message);
                }
            });
            _rabbitMqService.ListenForMessages("UserNotificationQueue", async message =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var projectService = scope.ServiceProvider.GetRequiredService<NotificationService>();
                    await projectService.SaveMessage(message);
                }
            });
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
            var clientId = (int)projectMessage.ClientId;
            var mail = await GetFreelancerMail(clientId);
            await SendEmailAsync(mail, "Create");
        }

        private async Task AcceptAsync(dynamic projectMessage)
        {
            var freelancerId = (int)projectMessage.FreelancerId;
            var mail = await GetFreelancerMail(freelancerId);
            await SendEmailAsync(mail, "Accept");
        }

        public async Task<string> GetAndRemoveMessage(string correlationId)
        {
            await Task.Delay(500);
            if (_pendingMessages.TryGetValue(correlationId, out var message))
            {
                _pendingMessages.Remove(correlationId);
                return message; 
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
                Action = "GetClientMail",
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

