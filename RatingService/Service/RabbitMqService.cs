using RabbitMQ.Client;

namespace RatingService.Service
{
    public class RabbitMqService
    {
        private readonly IConnection _connection;

        public RabbitMqService(IConnection connection)
        {
            _connection = connection;
        }

        public void Publish(string message)
        {
            using var channel = _connection.CreateModel();
            channel.QueueDeclare(queue: "reviewQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
            var body = System.Text.Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: "", routingKey: "reviewQueue", basicProperties: null, body: body);
        }
    }
}
