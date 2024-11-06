using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RatingService.Service
{
    public class RabbitMqService : IMessageBus
    {
        private readonly IModel _channel;

        public RabbitMqService(IModel channel)
        {
            _channel = channel;
            _channel.QueueDeclare(queue: "ResponseQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
        }

        public async Task PublishAsync(string queueName, string message)
        {
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: "", routingKey: queueName, body: body);
            await Task.CompletedTask;
        }


        public void ListenForMessages(string queueName, Func<string, Task> onMessageReceived)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                await onMessageReceived(message);
                _channel.BasicAck(ea.DeliveryTag, false);
            };
            _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
        }
    }

    public interface IMessageBus
    {
        Task PublishAsync(string queueName, string message);
        void ListenForMessages(string queueName, Func<string, Task> onMessageReceived);
    }
}
