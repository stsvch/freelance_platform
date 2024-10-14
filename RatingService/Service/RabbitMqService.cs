using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RatingService.Service
{
    public class RabbitMqService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMqService(IConnection connection)
        {
            _connection = connection;
            _channel = _connection.CreateModel();
        }

        // Метод для отправки сообщений в RabbitMQ
        public void Publish(string message, string queueName)
        {
            _channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
        }

        // Метод для прослушивания сообщений RabbitMQ
        public void ListenForMessages(string queueName, Action<string> onMessageReceived)
        {
            _channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                onMessageReceived(message);
            };

            _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
        }
    }
}
