using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ServiceDesk.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF Core tools (migrations). Avoids runtime service-provider requirements.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        var webPath = Path.Combine(basePath, "src", "ServiceDesk.Web");
        var configBasePath = Directory.Exists(webPath) ? webPath : basePath;

        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var configuration = new ConfigurationBuilder()
            .SetBasePath(configBasePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=localhost;Port=3306;Database=ticketing_system;User=servicedesk_app;Password=ServiceDesk!Dev2026;TreatTinyAsBoolean=true;";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        ServerVersion serverVersion;
        try
        {
            serverVersion = ServerVersion.AutoDetect(connectionString);
        }
        catch
        {
            serverVersion = new MariaDbServerVersion(new Version(10, 4, 32));
        }

        optionsBuilder.UseMySql(connectionString, serverVersion);
        return new AppDbContext(optionsBuilder.Options);
    }
}