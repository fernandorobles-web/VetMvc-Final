using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using VetMvc.Models;
using VetMvc.Services;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

// MVC con vistas
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<VetMvc.Services.AuthService>();

// EF Core (ajusta el nombre del connection string si es distinto)
builder.Services.AddDbContext<VetDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("VetDb")));
// Si en appsettings.json lo llamaste "VetDb", usa:



// Servicios propios
builder.Services.AddScoped<AuthService>();

// Autenticación con cookies
builder.Services
    .AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccesoDenegado";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        // options.Cookie.Name = "VetMvc.Auth"; // opcional
    });

var app = builder.Build();

// Cultura es-CL
var ci = new CultureInfo("es-CL");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(ci),
    SupportedCultures = new List<CultureInfo> { ci },
    SupportedUICultures = new List<CultureInfo> { ci }
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// MUY IMPORTANTE: primero autenticación, luego autorización
app.UseAuthentication();
app.UseAuthorization();

// Ruta por defecto
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
