using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using ServiceDesk.Application.Common;
using ServiceDesk.Application.Interfaces;
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

        return services;
    }
}
