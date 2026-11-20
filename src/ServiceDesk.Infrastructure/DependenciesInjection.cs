using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using ServiceDesk.Application.Authorization;
using ServiceDesk.Application.Common;
using ServiceDesk.Application.Interfaces;
using ServiceDesk.Application.Tickets;
using ServiceDesk.Infrastructure.Authorization;
using ServiceDesk.Infrastructure.Persistence;
using ServiceDesk.Infrastructure.Services;
using ServiceDesk.Infrastructure.Time;

namespace ServiceDesk.Infrastructure;

/// <summary>
/// Registers everything Infrastructure provides. Web calls this once at startup.
/// </summary>
public static class DependenciesInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<AppOptions>()
            .Bind(configuration.GetSection(AppOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<AppOptions>, AppOptionsValidator>();
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<IClock, SystemClock>();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                ServerVersion serverVersion;
                try
                {
                    serverVersion = ServerVersion.AutoDetect(connectionString);
                }
                catch
                {
                    // Design-time or unavailable DB: fall back to local XAMPP MariaDB version.
                    serverVersion = new MariaDbServerVersion(new Version(10, 4, 32));
                }

                options.UseMySql(connectionString, serverVersion);
            });
        }

        services.AddScoped<ITicketAccessScope, TicketAccessScope>();
        services.AddSingleton<IAttachmentStorage, AttachmentStorage>();
        services.AddScoped<ITicketNumberService, TicketNumberService>();
        services.AddScoped<ITicketService, TicketService>();

        return services;
    }
}
