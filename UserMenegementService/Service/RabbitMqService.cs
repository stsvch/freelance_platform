using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using System.Text;


namespace UserMenegementService.Service
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
        public async Task PublishMessageAsync(object message, string queueName)
        {
            var jsonMessage = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
        }

        // Метод для прослушивания очередей RabbitMQ
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
