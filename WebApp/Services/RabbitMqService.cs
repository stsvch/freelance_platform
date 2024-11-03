using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using Newtonsoft.Json;

namespace WebApp.Services
{
    public class RabbitMqService
    {
        private readonly IModel _channel;
        private readonly Dictionary<string, string> _pendingMessages = new();

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

        // Метод для извлечения и удаления сообщения из словаря по ключу (correlationId)
        public async Task<string> GetAndRemoveMessage(string correlationId)
        {
            await Task.Delay(500);
            // Проверяем, существует ли ключ в словаре
            if (_pendingMessages.TryGetValue(correlationId, out var message))
            {
                // Удаляем сообщение из словаря
                _pendingMessages.Remove(correlationId);
                return message; // Возвращаем извлеченное сообщение
            }

            return null; // Если сообщения с таким ключом нет, возвращаем null
        }

        public async Task ListenForMessagesAsync(string queueName)
        {
            _channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                // Десериализация сообщения для получения correlationId
                var response = JsonConvert.DeserializeObject<dynamic>(message);
                string correlationId = response.CorrelationId;

                _pendingMessages[correlationId] = message;

                // Подтверждение обработки сообщения
                _channel.BasicAck(ea.DeliveryTag, false);
            };

            _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
        }


    }

}
