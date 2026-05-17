using dWebShop.Application.Services;
using dWebShop.Domain.Entities.Users;
using dWebShop.Infrastructure.Persistence;
using dWebShop.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using dWebShop.Application.Common.Interfaces;

namespace dWebShop.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));
        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseMySql(connectionString, serverVersion,
                mySqlOptions => mySqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

        // Keep scoped registration for Identity and other scoped consumers
        services.AddScoped<AppDbContext>(sp =>
            sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

        services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
        {
            options.SignIn.RequireConfirmedEmail = false;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        services.AddScoped<AppDbContextInitializer>();
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddSingleton<IAppDbContextFactory, AppDbContextFactoryAdapter>();
        services.AddScoped<dWebShop.Application.Services.IPricingService, dWebShop.Infrastructure.Services.PricingService>();

        return services;
    }
}
