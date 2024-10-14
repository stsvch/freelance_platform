using Microsoft.EntityFrameworkCore;
using ProjectManagementService.Service;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Настройка базы данных MySQL
builder.Services.AddDbContext<ProjectDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(8, 0, 21))));

// Настройки RabbitMQ
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

builder.Services.AddSingleton<IModel>(sp =>
{
    var factory = sp.GetRequiredService<IConnectionFactory>();
    var connection = factory.CreateConnection();
    return connection.CreateModel();
});

builder.Services.AddSingleton<IMessageBus, RabbitMqMessageBus>();
builder.Services.AddScoped<IProjectService, ProjectService>();

builder.Services.AddControllers();

// Swagger (если нужно для тестирования)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Добавление миграций или создания базы данных
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProjectDbContext>();

    // Применение миграций при запуске приложения
    dbContext.Database.Migrate();
}

// Если в режиме разработки, включаем Swagger UI для тестирования API
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.MapControllers();

app.Run();




