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

    public class RabbitMqMessageBus : IMessageBus
    {
        private readonly IModel _channel;

        public RabbitMqMessageBus(IModel channel)
        {
            _channel = channel;

            // Настройка очереди (при необходимости)
            _channel.ExchangeDeclare(exchange: "project_exchange", type: ExchangeType.Topic);
        }

        // Публикация сообщений в RabbitMQ
        public Task PublishAsync(string topic, object message)
        {
            var jsonMessage = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            _channel.BasicPublish(
                exchange: "project_exchange",
                routingKey: topic,
                basicProperties: null,
                body: body
            );

            return Task.CompletedTask;
        }
    }

}
