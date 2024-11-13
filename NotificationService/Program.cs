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

builder.Services.AddSingleton<IConnection>(sp =>
{
    var factory = sp.GetRequiredService<IConnectionFactory>();
    return factory.CreateConnection();
});

builder.Services.AddSingleton<EmailService>();
builder.Services.AddSingleton<RabbitMqService>(); 
builder.Services.AddSingleton<NotifyService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var notifyService = scope.ServiceProvider.GetRequiredService<NotifyService>();
    notifyService.StartListeningForMessages();
}

app.UseRouting();

app.Run();




