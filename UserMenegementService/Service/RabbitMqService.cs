using Microsoft.EntityFrameworkCore.Metadata;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;


namespace UserMenegementService.Service
{
    public class RabbitMqService
    {
        private readonly IConnection connection;
        private readonly RabbitMQ.Client.IModel channel;

        public RabbitMqService(IConnection connection)
        {
            this.connection = connection;
            this.channel = connection.CreateModel();
            channel.QueueDeclare(queue: "UserRegisteredQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
        }

        public async Task PublishMessageAsync(object message, string queueName)
        {
            var jsonMessage = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
        }
    }
}
