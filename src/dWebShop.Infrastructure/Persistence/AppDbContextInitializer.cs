using dWebShop.Domain.Entities.Products;
using dWebShop.Domain.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace dWebShop.Infrastructure.Persistence;

public class AppDbContextInitializer
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AppDbContextInitializer> _logger;

    public AppDbContextInitializer(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<int>> roleManager,
        IConfiguration configuration,
        ILogger<AppDbContextInitializer> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task MigrateAsync()
    {
        await _context.Database.MigrateAsync();
    }

    public async Task SeedAsync()
    {
        await SeedRolesAsync();
        await SeedAdminUserAsync();
        await SeedBrandsAsync();
    }

    private async Task SeedRolesAsync()
    {
        foreach (var roleName in new[] { "Admin", "Client" })
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var result = await _roleManager.CreateAsync(new IdentityRole<int>(roleName));
                if (!result.Succeeded)
                    _logger.LogError("Failed to create role {Role}: {Errors}", roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    private async Task SeedAdminUserAsync()
    {
        var adminEmail = _configuration["Seed:AdminEmail"] ?? "admin@dwebshop.local";
        var adminPassword = _configuration["Seed:AdminPassword"] ?? "Admin@12345!";
        var adminUserName = _configuration["Seed:AdminUserName"] ?? "admin";

        if (await _userManager.FindByEmailAsync(adminEmail) is not null)
            return;

        var admin = new ApplicationUser
        {
            UserName = adminUserName,
            Email = adminEmail,
            EmailConfirmed = true,
            IsApproved = true
        };

        var result = await _userManager.CreateAsync(admin, adminPassword);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(admin, "Admin");
        }
        else
        {
            _logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    private async Task SeedBrandsAsync()
    {
        var brands = new[]
        {
            new Brand { Name = "STO",    Slug = "sto",    Description = "STO brand",    LogoImage = string.Empty, SliderImage = string.Empty },
            new Brand { Name = "XYPEX",  Slug = "xypex",  Description = "XYPEX brand",  LogoImage = string.Empty, SliderImage = string.Empty },
            new Brand { Name = "CORTEC", Slug = "cortec", Description = "CORTEC brand", LogoImage = string.Empty, SliderImage = string.Empty },
        };

        foreach (var brand in brands)
        {
            if (!await _context.Brands.AnyAsync(b => b.Slug == brand.Slug))
            {
                _context.Brands.Add(brand);
            }
        }

        await _context.SaveChangesAsync();
    }
}
