using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using MovieProject.Data;
using MovieProject.Models;
using MovieProject.Models.Settings;
using MovieProject.Services;
using MovieProject.Services.Interfaces;
using MovieProject.ViewModels;
using System.Configuration;


var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(ConnectionService.GetConnectionString(builder.Configuration)));


builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddDefaultUI()
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ApplicationDbContext>();

//Register a preconfigured MailSettings instance.
//Map to MailSettings instance from appsettings json file.
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddScoped<IMovieEmailSender, EmailService>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

//Register an AppSettings instance.
//Map to AppSettings instance from appsettings json file.
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

builder.Services.AddTransient<SeedService>();

//Register HttpClientFactory.
builder.Services.AddHttpClient();

//Register TmDbMovieService class.
builder.Services.AddScoped<IRemoteMovieService, TmDbMovieService>();

//Register TmDbMappingService class.
builder.Services.AddScoped<IDataMappingService, TmDbMappingService>();

//Register BasicImageService class.
builder.Services.AddScoped<IImageService, BasicImageService>();
builder.Services.AddSingleton<IImageService, BasicImageService>(); //singleton??????????

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

var dataService = app.Services
                     .CreateScope()
                     .ServiceProvider
                     .GetRequiredService<SeedService>();

await dataService.ManageDataAsync();

app.Run();
