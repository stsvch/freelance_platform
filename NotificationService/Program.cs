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

// ��������� ������� ����������� �����
builder.Services.AddSingleton<EmailService>();

var app = builder.Build();

// ������ ������������� ��������� RabbitMQ � ��������� ������
var rabbitMqService = app.Services.GetRequiredService<RabbitMqService>();
rabbitMqService.ListenForMessages();

// �������� �������������
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();

