using Microsoft.EntityFrameworkCore;
using PMS.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add DbContext to the service container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); // Use the appropriate connection string

// Add session services to the container
builder.Services.AddDistributedMemoryCache(); // Adds in-memory caching for session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout as per your requirement
    options.Cookie.HttpOnly = true; // Only accessible by the server
    options.Cookie.IsEssential = true; // Necessary for session management
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add session middleware to the request pipeline
app.UseSession(); // This should come before UseAuthorization

app.UseAuthorization();

//app.MapStaticAssets();

//PTenant
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=PTenant}/{action=PTenantHomePage}/{id?}");
//   .WithStaticAssets();

////Staff
//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Staff}/{action=SMaintenanceAssignment}/{id?}");

////property manager
//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=propertymanager}/{action=pmdashboard}/{id?}");

////Auth
//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Login}/{action=Login}/{id?}");

app.Run();
