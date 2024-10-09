using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RabbitMQ.Client;
using System.Configuration;
using System.Text;
using Pomelo.EntityFrameworkCore.MySql;
using UserMenegementService.Service;

var builder = WebApplication.CreateBuilder(args);

// 1. ��������� ����������� � ���� ������ (MySQL)
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(8, 0, 21))));

// 2. ����������� �������� ��� Dependency Injection (DI)
builder.Services.AddScoped<IUserService, UserService>();      // ������ ��� ���������� ��������������
builder.Services.AddScoped<IProfileService, ProfileService>();  // ������ ��� ���������� ���������

builder.Services.AddSingleton(sp =>
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

// 4. ��������� JWT ��������������
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

// 5. ��������� ������������
builder.Services.AddControllers();

// 6. ���������� Swagger (���� ����� ��� ������������ API)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ���������� �������� ��� �������� ���� ������
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();

    // ���������� �������� ��� ������� ���������� (���� ������������ ��������)
    dbContext.Database.Migrate();

    // ���� ����� ������������ EnsureCreated(), ���� �������� �� ������������:
    // dbContext.Database.EnsureCreated();
}

// 7. ���� � ������ ����������, �������� Swagger UI ��� ������������ API
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 8. ���������� middleware ��� ������ ����������
app.UseHttpsRedirection();

app.UseRouting();

// ��������� �������������� � �����������
app.UseAuthentication();
app.UseAuthorization();

// ������������� ������������
app.MapControllers();

app.Run();


