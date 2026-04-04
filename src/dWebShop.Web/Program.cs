using dWebShop.Application;
using MudBlazor.Services;
using dWebShop.Domain.Entities.Users;
using dWebShop.Infrastructure;
using dWebShop.Infrastructure.Persistence;
using dWebShop.Web.Components;
using Microsoft.AspNetCore.Identity;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .AddEnvironmentVariables()
        .Build())
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/dwebshop-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddMemoryCache();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/access-denied";
    options.Cookie.HttpOnly = true;
    options.SlidingExpiration = true;
});

builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

// Initialize database and seed data
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<AppDbContextInitializer>();
    await initializer.MigrateAsync();
    await initializer.SeedAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseStatusCodePagesWithReExecute("/not-found");

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Logout endpoint
app.MapPost("/logout", async (SignInManager<ApplicationUser> signInManager, HttpContext context) =>
{
    await signInManager.SignOutAsync();
    return Results.Redirect("/login");
}).RequireAuthorization();

app.Run();
