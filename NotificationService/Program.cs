using NotificationService.Service;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IConnectionFactory>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new ConnectionFactory
    {
        HostName = config["RabbitMQ:HostName"],
        Port = int.Parse(config["RabbitMQ:Port"]),
        UserName = config["RabbitMQ:UserName"],
        Password = config["RabbitMQ:Password"]
    };
});

// Настройка сервиса электронной почты
builder.Services.AddSingleton<EmailService>();

var app = builder.Build();

// Запуск прослушивания сообщений RabbitMQ в отдельном потоке
var rabbitMqService = app.Services.GetRequiredService<RabbitMqService>();
rabbitMqService.ListenForMessages();

// Добавьте маршрутизацию
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();

