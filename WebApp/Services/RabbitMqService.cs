using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;

namespace WebApp.Services
{
    public class RabbitMqService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly UserService _userService; // Сервис для работы с пользователями

        public RabbitMqService(IConnection connection, UserService userService)
        {
            _connection = connection;
            _channel = _connection.CreateModel();
            _userService = userService;

            // Создаем очереди
            _channel.QueueDeclare(queue: "userRegistrationQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueDeclare(queue: "userLoginQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
        }

        public void ListenForMessages()
        {
            var registrationConsumer = new EventingBasicConsumer(_channel);
            registrationConsumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var registrationData = JsonConvert.DeserializeObject<UserRegisterModel>(message);

                // Обработка регистрации
                await _userService.RegisterUserAsync(registrationData);
            };

            _channel.BasicConsume(queue: "userRegistrationQueue", autoAck: true, consumer: registrationConsumer);

            var loginConsumer = new EventingBasicConsumer(_channel);
            loginConsumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var loginData = JsonConvert.DeserializeObject<UserLoginModel>(message);

                // Обработка авторизации
                await _userService.AuthenticateAsync(loginData);
            };

            _channel.BasicConsume(queue: "userLoginQueue", autoAck: true, consumer: loginConsumer);
        }
    }

}
