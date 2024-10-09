using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json;
using System.Text;
using RabbitMQ.Client;
using IModel = RabbitMQ.Client.IModel;

namespace ProjectManagementService.Service
{
    // Messaging/IMessageBus.cs
    public interface IMessageBus
    {
        Task PublishAsync(string topic, object message);
    }

    // Messaging/RabbitMqMessageBus.cs
    public class RabbitMqMessageBus : IMessageBus
    {
        private readonly RabbitMQ.Client.IModel _channel;

        public RabbitMqMessageBus(IModel channel)
        {
            _channel = channel;
        }

        public async Task PublishAsync(string topic, object message)
        {
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            _channel.BasicPublish(exchange: "project_exchange",
                                   routingKey: topic,
                                   basicProperties: null,
                                   body: body);
            await Task.CompletedTask;
        }
    }

}
