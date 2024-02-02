using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MaHakesherServerSide.Data;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<MaHakesherServerSideContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MaHakesherServerSideContext") ?? throw new InvalidOperationException("Connection string 'MaHakesherServerSideContext' not found.")));


// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Allow All",
        builder =>
        {
            builder.WithOrigins("http://localhost:3000").AllowAnyMethod().AllowAnyHeader().AllowCredentials();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseCors("Allow All");

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
