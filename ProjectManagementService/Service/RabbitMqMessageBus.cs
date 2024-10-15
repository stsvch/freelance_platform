using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json;
using System.Text;
using RabbitMQ.Client;
using IModel = RabbitMQ.Client.IModel;
using RabbitMQ.Client.Events;

namespace ProjectManagementService.Service
{
    // Messaging/IMessageBus.cs
    // Services/IMessageBus.cs
    public interface IMessageBus
    {
        Task PublishAsync(string queueName, string message);
        void ListenForMessages(string queueName, Func<string, Task> onMessageReceived);
    }

    public class RabbitMqMessageBus : IMessageBus
    {
        private readonly IModel _channel;

        public RabbitMqMessageBus(IModel channel)
        {
            _channel = channel;
            // Объявляем очередь (если она еще не существует)
            // Делаем очередь "ProjectQueue" доступной
            _channel.QueueDeclare(queue: "ProjectQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);

            // Делаем очередь "ProjectResponseQueue" доступной для отправки ответов
            _channel.QueueDeclare(queue: "ProjectResponseQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
        }

        // Отправка сообщения в очередь
        public async Task PublishAsync(string queueName, string message)
        {
            _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
            await Task.CompletedTask;
        }

        // Прослушивание сообщений из очереди
        public void ListenForMessages(string queueName, Func<string, Task> onMessageReceived)
        {
            _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                // Обработка полученного сообщения
                onMessageReceived(message);
            };

            // Начинаем прослушивание очереди
            _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
        }
    }

}
