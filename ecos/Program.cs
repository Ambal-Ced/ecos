using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ecos.Areas.Identity.Data;
using ecos.Services;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Set up the connection string.
var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection")
    ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");

// Configure services.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register HttpClient and OpenAIService
builder.Services.AddHttpClient<OpenAIService>();  // This will provide HttpClient dependency to OpenAIService
builder.Services.AddSingleton<OpenAIService>();   // This is now correctly registered
builder.Services.AddHttpClient<CohereService>(client =>
{
    client.BaseAddress = new Uri("https://api.cohere.ai/");
});

// Add Identity services with custom ApplicationUser.
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Add services to the container (e.g., MVC controllers).
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication(); // Enables authentication middleware
app.UseAuthorization();  // Enables authorization middleware

// Map routes and Razor Pages
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
