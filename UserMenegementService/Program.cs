using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RabbitMQ.Client;
using System.Text;
using UserMenegementService.Service;

var builder = WebApplication.CreateBuilder(args);

// Настройка подключения к RabbitMQ
builder.Services.AddSingleton<IConnection>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var factory = new ConnectionFactory()
    {
        HostName = config["RabbitMQ:HostName"],
        Port = int.Parse(config["RabbitMQ:Port"]),
        UserName = config["RabbitMQ:UserName"],
        Password = config["RabbitMQ:Password"]
    };
    return factory.CreateConnection();
});

builder.Services.AddSingleton<RabbitMqService>();  // Сервис для RabbitMQ
builder.Services.AddScoped<IUserService, UserService>();  // Сервис для пользователей
builder.Services.AddScoped<IProfileService, ProfileService>();  // Сервис для профилей

// Настройка базы данных MySQL
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(8, 0, 21))));

// Настройка JWT аутентификации
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

// Настройка контроллеров
builder.Services.AddControllers();

// Настройка Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Запуск прослушивания очередей RabbitMQ
var userService = app.Services.GetRequiredService<UserService>();
userService.ListenForUserRegistrationMessages();
userService.ListenForUserLoginMessages();

// Добавление миграций или создания базы данных
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();

    // Применение миграций при запуске приложения
    dbContext.Database.Migrate();
}

// Добавление middleware
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Настройка маршрутов
app.MapControllers();

// Включение Swagger UI для тестирования API
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();



