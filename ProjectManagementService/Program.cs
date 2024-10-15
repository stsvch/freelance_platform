using Microsoft.EntityFrameworkCore;
using ProjectManagementService.Service;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// ��������� ���� ������ MySQL
builder.Services.AddDbContext<ProjectDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(8, 0, 21))));

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

builder.Services.AddSingleton<IMessageBus, RabbitMqMessageBus>(); // ����������� RabbitMqMessageBus
builder.Services.AddScoped<IProjectService, ProjectService>(); // ����������� ProjectService

builder.Services.AddControllers();

// Swagger (���� ����� ��� ������������)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ���������� �������� ��� �������� ���� ������
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProjectDbContext>();

    // ���������� �������� ��� ������� ����������
    dbContext.Database.Migrate();
}

// ���� � ������ ����������, �������� Swagger UI ��� ������������ API
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.MapControllers();

app.Run();

