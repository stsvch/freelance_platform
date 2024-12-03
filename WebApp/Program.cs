using RabbitMQ.Client;
using WebApp.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddConsole();

builder.Services.AddHttpClient();

builder.Services.AddScoped<RoleFilter>();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<RoleFilter>(); 
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); 
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

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

builder.Services.AddSingleton<RabbitMqService>();
builder.Services.AddSingleton<ProjectService>();
builder.Services.AddSingleton<ProfileService>();
builder.Services.AddScoped<ReviewService>();
builder.Services.AddScoped<ResponseService>();
builder.Services.AddScoped<AuthService>();  

var app = builder.Build();

var rabbitMqService = app.Services.GetRequiredService<RabbitMqService>();
_ = rabbitMqService.ListenForMessagesAsync("ProjectResponseQueue");

app.UseSession();

//app.UseHttpsRedirection();
app.UseStaticFiles(); 
app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapDefaultControllerRoute();
});

app.Run();








