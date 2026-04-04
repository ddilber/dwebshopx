using dWebShop.DataMigration;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddEnvironmentVariables()
    .Build();

var srcConn = config.GetConnectionString("SourceConnection")
    ?? throw new InvalidOperationException("SourceConnection is required in appsettings.json");
var tgtConn = config.GetConnectionString("TargetConnection")
    ?? throw new InvalidOperationException("TargetConnection is required in appsettings.json");

var srcImagesPath = config["Migration:SourceImagesPath"];
var srcDocsPath   = config["Migration:SourceDocsPath"];
var tgtImagesPath = config["Migration:TargetImagesPath"];
var tgtDocsPath   = config["Migration:TargetDocsPath"];

Console.WriteLine("=== dWebShopX Data Migration ===");
Console.WriteLine($"Source: {Mask(srcConn)}");
Console.WriteLine($"Target: {Mask(tgtConn)}");
Console.WriteLine();

try
{
    var runner = new MigrationRunner(srcConn, tgtConn, srcImagesPath, srcDocsPath, tgtImagesPath, tgtDocsPath);
    await runner.RunAsync();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine($"Migration failed: {ex.Message}");
    Console.ResetColor();
    return 1;
}

return 0;

static string Mask(string conn)
{
    // Redact password from connection string for display
    var parts = conn.Split(';');
    return string.Join(';', parts.Select(p =>
        p.TrimStart().StartsWith("Password=", StringComparison.OrdinalIgnoreCase) ? "Password=***" : p));
}
