using Microsoft.EntityFrameworkCore;
using PMS.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add DbContext to the service container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); // Use the appropriate connection string

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

app.UseAuthorization();

//app.MapStaticAssets();

////PTenant
//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=PTenant}/{action=PTenantHomePage}/{id?}");
////   .WithStaticAssets();

////Staff
//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Staff}/{action=SMaintenanceAssignment}/{id?}");

//property manager
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=propertymanager}/{action=pmdashboard}/{id?}");

////Auth
//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Login}/{action=Login}/{id?}")
//    .WithStaticAssets();

app.Run();
