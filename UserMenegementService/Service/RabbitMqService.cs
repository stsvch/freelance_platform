using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using System.Text;


namespace UserMenegementService.Service
{
    public class RabbitMqService : IMessageBus
    {
        private readonly IModel _channel;
        private readonly IServiceScopeFactory _scopeFactory;

        public RabbitMqService(IModel channel, IServiceScopeFactory scopeFactory)
        {
            _channel = channel;
            _channel.QueueDeclare(queue: "UserNotificationQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueDeclare(queue: "NotificationUserQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
            _scopeFactory = scopeFactory;
        }

        public async Task PublishAsync(string queueName, string message)
        {
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: "", routingKey: queueName, body: body);
            await Task.CompletedTask;
        }


        public void ListenForMessages(string queueName)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                // Создаем область для Scoped-сервиса
                using (var scope = _scopeFactory.CreateScope())
                {
                    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                    await userService.Handle(message); // Вызываем метод-обработчик
                }

                _channel.BasicAck(ea.DeliveryTag, false);
            };

            _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
        }
    }

    public interface IMessageBus
    {
        Task PublishAsync(string queueName, string message);
        void ListenForMessages(string queueName);
    }
}
