using dWebShop.Application;
using dWebShop.Domain.Entities.Users;
using dWebShop.Infrastructure;
using dWebShop.Infrastructure.Persistence;
using dWebShop.Admin.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.FluentUI.AspNetCore.Components;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .AddEnvironmentVariables()
        .Build())
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/dwebshop-admin-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();
builder.WebHost.UseStaticWebAssets();

builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddFluentUIComponents();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/access-denied";
    options.Cookie.HttpOnly = true;
    options.SlidingExpiration = true;
});

builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<AppDbContextInitializer>();
    await initializer.MigrateAsync();
    await initializer.SeedAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapPost("/account/login", async (
    HttpContext ctx,
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager) =>
{
    var form = await ctx.Request.ReadFormAsync();
    var email = form["email"].ToString();
    var password = form["password"].ToString();
    var returnUrl = form["returnUrl"].ToString();

    var user = await userManager.FindByEmailAsync(email);
    var userName = user?.UserName ?? email;
    var result = await signInManager.PasswordSignInAsync(userName, password, isPersistent: false, lockoutOnFailure: true);

    if (result.Succeeded)
        return Results.Redirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);

    var error = result.IsLockedOut ? "Account is locked. Try again later." : "Invalid email or password.";
    return Results.Redirect($"/login?error={Uri.EscapeDataString(error)}");
}).DisableAntiforgery();

app.MapPost("/logout", async (SignInManager<ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Redirect("/login");
}).RequireAuthorization();

app.Run();
