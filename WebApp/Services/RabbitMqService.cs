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

        public async Task<string> WaitForMessageAsync(string responseQueue,  int timeoutMilliseconds = 10000)
        {
            var tcs = new TaskCompletionSource<string>();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var props = ea.BasicProperties;

                Console.WriteLine($"Получено сообщение с CorrelationId: {props.CorrelationId}");

                tcs.SetResult(message); // Устанавливаем результат задачи
            };

            _channel.BasicConsume(queue: responseQueue,
                                 autoAck: true,
                                 consumer: consumer);

            // Ожидание с таймаутом
            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(timeoutMilliseconds));

            if (completedTask == tcs.Task)
            {
                return await tcs.Task; // Сообщение успешно получено
            }
            else
            {
                throw new TimeoutException("Превышено время ожидания ответа.");
            }
        }

    }

}
