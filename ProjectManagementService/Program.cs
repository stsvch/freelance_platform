var builder = WebApplication.CreateBuilder(args);

// Добавление сервисов HttpClient для взаимодействия с микросервисами
builder.Services.AddHttpClient("UserMenegementService", client =>
{
    client.BaseAddress = new Uri("http://localhost:5001/api/");
});
builder.Services.AddHttpClient("ProjectManagementService", client =>
{
    client.BaseAddress = new Uri("http://localhost:5002/api/");
});
builder.Services.AddHttpClient("SearchService", client =>
{
    client.BaseAddress = new Uri("http://localhost:5003/api/");
});
builder.Services.AddHttpClient("RatingService", client =>
{
    client.BaseAddress = new Uri("http://localhost:5004/api/");
});
builder.Services.AddHttpClient("NotificationService", client =>
{
    client.BaseAddress = new Uri("http://localhost:5005/api/");
});

// Если это Web API, то используем AddControllers
builder.Services.AddControllers();

var app = builder.Build();

// Middleware для работы со статическими файлами
app.UseStaticFiles();

// Включаем маршрутизацию
app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers(); // Для API
});

app.Run();




