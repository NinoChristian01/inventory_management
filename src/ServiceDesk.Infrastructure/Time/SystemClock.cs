using Microsoft.Extensions.Options;
using ServiceDesk.Application.Common;
using ServiceDesk.Application.Interfaces;

namespace ServiceDesk.Infrastructure.Time;

/// <summary>
/// <see cref="IClock"/> on top of <see cref="TimeProvider"/>, so tests can freeze or move time
/// with a fake provider. The time zone comes from <see cref="AppOptions.TimeZone"/>.
/// </summary>
public sealed class SystemClock : IClock
{
    private readonly TimeProvider _timeProvider;

    public SystemClock(TimeProvider timeProvider, IOptions<AppOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _timeProvider = timeProvider;
        TimeZone = TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZone);
    }

    public TimeZoneInfo TimeZone { get; }

    public DateTime UtcNow => _timeProvider.GetUtcNow().UtcDateTime;

    public DateTime LocalNow => ToLocal(UtcNow);

    public DateOnly LocalToday => DateOnly.FromDateTime(LocalNow);

    public DateTime ToLocal(DateTime utc)
    {
        var asUtc = utc.Kind switch
        {
            DateTimeKind.Utc => utc,
            DateTimeKind.Local => utc.ToUniversalTime(),
            // The database stores UTC without a kind, so unspecified means UTC here.
            _ => DateTime.SpecifyKind(utc, DateTimeKind.Utc),
        };
        return TimeZoneInfo.ConvertTimeFromUtc(asUtc, TimeZone);
    }

    public DateTime ToUtc(DateTime local) =>
        TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), TimeZone);
}
