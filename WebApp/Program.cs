var builder = WebApplication.CreateBuilder(args);

var baseApiUri = "http://localhost:";

builder.Services.AddHttpClient("UserManagementService", client =>
{
    client.BaseAddress = new Uri($"{baseApiUri}5001/api/");
});
builder.Services.AddHttpClient("ProjectManagementService", client =>
{
    client.BaseAddress = new Uri($"{baseApiUri}5002/api/");
});
builder.Services.AddHttpClient("SearchService", client =>
{
    client.BaseAddress = new Uri($"{baseApiUri}5003/api/");
});
builder.Services.AddHttpClient("RatingService", client =>
{
    client.BaseAddress = new Uri($"{baseApiUri}5004/api/");
});
builder.Services.AddHttpClient("NotificationService", client =>
{
    client.BaseAddress = new Uri($"{baseApiUri}5005/api/");
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseStaticFiles();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Auth}/{action=Login}/{id?}");
    endpoints.MapControllerRoute(
        name: "profile",
        pattern: "Profile/{action=ClientProfile}/{id?}",
        defaults: new { controller = "Profile" });
});

// Запуск приложения
app.Run();



