using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RatingService.Service
{
    public class RabbitMqService : IMessageBus
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private const string ExchangeName = "FanoutExchange";

        public RabbitMqService(IConnectionFactory factory)
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Декларация очередей и обмена
            _channel.QueueDeclare(queue: "ResponseToProjectQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueDeclare(queue: "ResponseToNotificationQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
            _channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Fanout);

            // Привязка очередей к обмену
            _channel.QueueBind(queue: "ResponseToProjectQueue", exchange: ExchangeName, routingKey: "");
            _channel.QueueBind(queue: "ResponseToNotificationQueue", exchange: ExchangeName, routingKey: "");
        }

        public async Task PublishAsync(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: ExchangeName, routingKey: "", body: body);
            await Task.CompletedTask;
        }

        // Закрытие канала и соединения при завершении работы приложения
        public void Close()
        {
            _channel.Close();
            _connection.Close();
        }
    }

    public interface IMessageBus
    {
        Task PublishAsync(string message);
    }
}

