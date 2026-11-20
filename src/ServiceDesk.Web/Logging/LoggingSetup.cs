using System.Globalization;
using Serilog;
using Serilog.Events;

namespace ServiceDesk.Web.Logging;

/// <summary>
/// Serilog setup. Levels come from the "Serilog" section of appsettings.json; file sinks are
/// configured here so their paths are always under the app's content root, which also holds
/// under IIS where the working directory is not the site folder.
/// </summary>
public static class LoggingSetup
{
    /// <summary>
    /// Logger for failures before the host starts or outside a request: console plus
    /// <c>logs/fatal-*.log</c> next to the app's binaries.
    /// </summary>
    public static Serilog.ILogger CreateFatalLogger() => new LoggerConfiguration()
        .MinimumLevel.Warning()
        .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
        .WriteTo.File(
            Path.Combine(AppContext.BaseDirectory, "logs", "fatal-.log"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            formatProvider: CultureInfo.InvariantCulture)
        .CreateLogger();

    /// <summary>
    /// The app's main logger. It's owned by the host (the static <see cref="Log.Logger"/> is left
    /// alone), so several in-memory test hosts can run in one process.
    /// </summary>
    public static IServiceCollection AddServiceDeskLogging(this IServiceCollection services)
    {
        services.AddSerilog(
            (provider, logger) =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                var environment = provider.GetRequiredService<IHostEnvironment>();
                var files = configuration.GetSection(LogFileOptions.SectionName).Get<LogFileOptions>() ?? new LogFileOptions();

                logger.ReadFrom.Configuration(configuration)
                    .ReadFrom.Services(provider)
                    .Enrich.FromLogContext()
                    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture);

                if (!files.Enabled)
                {
                    return;
                }

                var directory = Path.IsPathRooted(files.Directory)
                    ? files.Directory
                    : Path.Combine(environment.ContentRootPath, files.Directory);

                logger
                    .WriteTo.File(
                        Path.Combine(directory, "servicedesk-.log"),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: files.RetainedDays,
                        formatProvider: CultureInfo.InvariantCulture)
                    .WriteTo.File(
                        Path.Combine(directory, "errors-.log"),
                        restrictedToMinimumLevel: LogEventLevel.Error,
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: files.RetainedDays * 3,
                        formatProvider: CultureInfo.InvariantCulture);
            },
            preserveStaticLogger: true);

        return services;
    }
}
