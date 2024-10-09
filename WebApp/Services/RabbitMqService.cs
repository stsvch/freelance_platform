using System.Text;

namespace WebApp.Services
{
    public class RabbitMqService
    {
        private readonly IModel _channel;

        public RabbitMqService(IConnection connection)
        {
            _channel = connection.CreateModel();
        }

        public void PublishMessage(string message, string queueName)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
        }

        public void ListenForMessages(string queueName, Action<string> onMessageReceived)
        {
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
