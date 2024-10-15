using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ProjectManagementService.Service;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IProjectService, ProjectService>();


builder.Services.AddDbContext<ProjectDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 39))));


// Настройка RabbitMQ
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
    return factory.CreateConnection(); // Создаем подключение к RabbitMQ
});

builder.Services.AddSingleton<IModel>(sp =>
{
    var connection = sp.GetRequiredService<IConnection>();
    return connection.CreateModel(); // Создаем канал для обмена сообщениями
});

// Регистрируем IMessageBus как Singleton
builder.Services.AddSingleton<IMessageBus, RabbitMqMessageBus>();


builder.Services.AddControllers();

var app = builder.Build();

// Добавление миграций или создание базы данных
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProjectDbContext>();
    dbContext.Database.Migrate();
}

// Теперь после создания всех сервисов вызываем StartListeningForMessages()
using (var scope = app.Services.CreateScope())
{
    var projectService = scope.ServiceProvider.GetRequiredService<IProjectService>();
    if (projectService is ProjectService service)
    {
        service.StartListeningForMessages(); // Начинаем прослушивание сообщений
    }
}

app.UseRouting();
app.MapControllers();

app.Run();





