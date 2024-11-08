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
builder.Services.AddSingleton<NotificationService.Service.NotificationService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var projectService = scope.ServiceProvider.GetRequiredService<NotificationService.Service.NotificationService>();
    if (projectService is NotificationService.Service.NotificationService service)
    {
        service.StartListeningForMessages(); // Начинаем прослушивание сообщений
    }
}

// Добавьте маршрутизацию
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();

