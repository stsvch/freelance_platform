using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RatingService.Service;

var builder = WebApplication.CreateBuilder(args);

// ���������� ��������� ���� ������ (MySQL)
builder.Services.AddDbContext<ReviewDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(8, 0, 21))));

// ��������� RabbitMQ
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

// ���������� ��������
builder.Services.AddScoped<ReviewService>();
builder.Services.AddScoped<ResponseService>();
builder.Services.AddSingleton<IMessageBus, RabbitMqService>();

var app = builder.Build();

// ��������/�������� ���� ������ ��� �������
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ReviewDbContext>();
    dbContext.Database.Migrate();
}

// �������������
app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});


app.Run();


