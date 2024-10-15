using RabbitMQ.Client;
using WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Настройки RabbitMQ с использованием IConfiguration
builder.Services.AddSingleton<IConnection>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var factory = new ConnectionFactory
    {
        HostName = config["RabbitMQ:HostName"],
        Port = int.Parse(config["RabbitMQ:Port"]),
        UserName = config["RabbitMQ:UserName"],
        Password = config["RabbitMQ:Password"]
    };
    return factory.CreateConnection();
});

builder.Services.AddSingleton<IModel>(sp =>
{
    var connection = sp.GetRequiredService<IConnection>();
    return connection.CreateModel();
});

// Настроим RabbitMqService для работы с RabbitMQ
builder.Services.AddSingleton<RabbitMqService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();
app.UseStaticFiles(); // Статические файлы (CSS, JS)

app.UseEndpoints(endpoints =>
{
    endpoints.MapDefaultControllerRoute();
});

app.Run();







