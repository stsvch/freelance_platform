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


// ��������� RabbitMQ
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
    return factory.CreateConnection(); // ������� ����������� � RabbitMQ
});

builder.Services.AddSingleton<IModel>(sp =>
{
    var connection = sp.GetRequiredService<IConnection>();
    return connection.CreateModel(); // ������� ����� ��� ������ �����������
});

// ������������ IMessageBus ��� Singleton
builder.Services.AddSingleton<IMessageBus, RabbitMqMessageBus>();


builder.Services.AddControllers();

var app = builder.Build();

// ���������� �������� ��� �������� ���� ������
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProjectDbContext>();
    dbContext.Database.Migrate();
}

// ������ ����� �������� ���� �������� �������� StartListeningForMessages()
using (var scope = app.Services.CreateScope())
{
    var projectService = scope.ServiceProvider.GetRequiredService<IProjectService>();
    if (projectService is ProjectService service)
    {
        service.StartListeningForMessages(); // �������� ������������� ���������
    }
}

app.UseRouting();
app.MapControllers();

app.Run();





