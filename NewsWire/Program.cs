using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NewsWire.Data;
using NewsWire.Models;
using NewsWire.Services;

var builder = WebApplication.CreateBuilder(args);

// Database Service
builder.Services.AddDbContext<NewsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity Service Setup
builder.Services.AddIdentity<User, IdentityRole>(
     option =>
     {
         option.Password.RequireDigit = false;
         option.Password.RequireLowercase = false;
         option.Password.RequireUppercase = false;
         option.Password.RequireNonAlphanumeric = false;
         option.Password.RequiredLength = 6;
         option.User.RequireUniqueEmail = false;
         option.SignIn.RequireConfirmedEmail = false;
         option.SignIn.RequireConfirmedPhoneNumber = false;
     })
    .AddEntityFrameworkStores<NewsDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<INewsManagementService, NewsManagementService>();

builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Categories}/{action=Index}/{id?}");

app.Run();