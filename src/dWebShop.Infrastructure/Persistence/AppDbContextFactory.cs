using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace dWebShop.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(
                "Server=172.24.30.229;Port=3306;Database=dWebShopX;User=root;Password=mysqlisok;",
                ServerVersion.AutoDetect("Server=172.24.30.229;Port=3306;Database=dWebShopX;User=root;Password=mysqlisok;"))
            .Options;
        return new AppDbContext(options);
    }
}
