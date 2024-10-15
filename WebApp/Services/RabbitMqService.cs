using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;

namespace WebApp.Services
{
    public class RabbitMqService
    {
        private readonly IModel _channel;

        public RabbitMqService(IModel channel)
        {
            _channel = channel;
        }

        // Отправка сообщения в очередь RabbitMQ
        public void PublishMessage(string queueName, string message, string correlationId)
        {
            var properties = _channel.CreateBasicProperties();
            properties.CorrelationId = correlationId;

            _channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: properties, body: body);
        }

        // Прослушивание сообщений из очереди RabbitMQ
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
