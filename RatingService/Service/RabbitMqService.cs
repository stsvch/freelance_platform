using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RatingService.Service
{
    public class RabbitMqService : IMessageBus
    {
        private readonly IModel _channel;
        private const string ExchangeName = "FanoutExchange";


        public RabbitMqService(IModel channel)
        {
            _channel = channel;
            _channel.QueueDeclare(queue: "ResponseToProjectQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueDeclare(queue: "ResponseToNotificationQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
            _channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Fanout);
            _channel.QueueBind(queue: "ResponseToProjectQueue", exchange: ExchangeName, routingKey: "");
            _channel.QueueBind(queue: "ResponseToNotificationQueue", exchange: ExchangeName, routingKey: "");
        }

        public async Task PublishAsync(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: ExchangeName, routingKey: "", body: body);
            await Task.CompletedTask;
        }
    }

    public interface IMessageBus
    {
        Task PublishAsync(string message);
    }
}
