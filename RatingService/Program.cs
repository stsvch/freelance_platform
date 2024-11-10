using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RatingService.Service;

var builder = WebApplication.CreateBuilder(args);

// ���������� ������������ ��� RabbitMQ
builder.Services.AddSingleton<IConnectionFactory>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var factory = new ConnectionFactory
    {
        HostName = config["RabbitMQ:HostName"],
        Port = int.Parse(config["RabbitMQ:Port"]),
        UserName = config["RabbitMQ:UserName"],
        Password = config["RabbitMQ:Password"]
    };
    return factory;
});

// ������������ RabbitMqService ��� Singleton
builder.Services.AddSingleton<IMessageBus, RabbitMqService>();

// ����������� ��������� ���� ������ (MySQL)
builder.Services.AddDbContext<ReviewDbContext>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 21))
    );
});

builder.Services.AddScoped<ReviewService>();
builder.Services.AddScoped<ResponseService>();

builder.Services.AddLogging();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ReviewDbContext>();
    try
    {
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
    }
}
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.MapControllers();  

app.Run();















