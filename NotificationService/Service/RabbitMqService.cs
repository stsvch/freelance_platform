using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using NotificationService.Model;

namespace NotificationService.Service
{
    public class RabbitMqService
    {
        private readonly IConnection _connection;
        private readonly EmailService _emailService;

        public RabbitMqService(IConnection connection, EmailService emailService)
        {
            _connection = connection;
            _emailService = emailService;
        }

        public void ListenForMessages()
        {
            using var channel = _connection.CreateModel();
            channel.QueueDeclare(queue: "notificationQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var notification = JsonSerializer.Deserialize<Notification>(message);
                if (notification != null)
                {
                    await _emailService.SendEmailAsync(notification);
                }
            };

            channel.BasicConsume(queue: "notificationQueue", autoAck: true, consumer: consumer);
        }
    }
}
