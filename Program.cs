using TodoApp.Data;
using Microsoft.EntityFrameworkCore;
using System.IO;

var options = new WebApplicationOptions
{
    ContentRootPath = AppContext.BaseDirectory,
    WebRootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot")
};

var builder = WebApplication.CreateBuilder(options);

// Pobierz ConnectionString z konfiguracji
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Połącz bazę danych z pełną ścieżką względem katalogu aplikacji
var dbPath = Path.Combine(AppContext.BaseDirectory, connectionString.Replace("Data Source=", ""));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// Logowanie ścieżki do bazy danych dla pewności
Console.WriteLine($"Using database at: {dbPath}");

var app = builder.Build();

// Log the paths to ensure they are correctly set
Console.WriteLine($"WebRootPath: {app.Environment.WebRootPath}");
Console.WriteLine($"ContentRootPath: {app.Environment.ContentRootPath}");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Tasks}/{action=Index}/{id?}");

app.Run();
