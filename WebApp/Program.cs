using RabbitMQ.Client;
using WebApp.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
builder.Services.AddScoped<RoleFilter>();
// Включаем сессии
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Время жизни сессии
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Сессия будет работать даже с блокировщиками куков
});

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

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); 
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddSingleton<RabbitMqService>();
builder.Services.AddSingleton<ProjectService>();
builder.Services.AddSingleton<ProfileService>(); // или AddScoped<ProfileService>()



builder.Services.AddControllersWithViews();

var app = builder.Build();

var rabbitMqService = app.Services.GetRequiredService<RabbitMqService>();
_ = rabbitMqService.ListenForMessagesAsync("ProjectResponseQueue");

app.UseSession();

app.UseHttpsRedirection();
app.UseRouting();
app.UseStaticFiles(); // Статические файлы (CSS, JS)

app.UseEndpoints(endpoints =>
{
    endpoints.MapDefaultControllerRoute();
});

app.Run();







