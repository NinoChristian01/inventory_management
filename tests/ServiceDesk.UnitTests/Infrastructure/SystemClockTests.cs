using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using ServiceDesk.Application.Common;
using ServiceDesk.Infrastructure.Time;

namespace ServiceDesk.UnitTests.Infrastructure;

public sealed class SystemClockTests
{
    private static SystemClock CreateClock(DateTimeOffset utcNow, string timeZone = "Asia/Manila") =>
        new(new FakeTimeProvider(utcNow), Options.Create(new AppOptions { TimeZone = timeZone, OrganizationName = "MIS" }));

    [Fact]
    public void Utc_now_comes_from_the_time_provider()
    {
        var clock = CreateClock(new DateTimeOffset(2026, 11, 20, 2, 15, 0, TimeSpan.Zero));

        Assert.Equal(new DateTime(2026, 11, 20, 2, 15, 0, DateTimeKind.Utc), clock.UtcNow);
        Assert.Equal(DateTimeKind.Utc, clock.UtcNow.Kind);
    }

    [Fact]
    public void Manila_is_eight_hours_ahead_of_utc()
    {
        var clock = CreateClock(DateTimeOffset.UnixEpoch);

        var local = clock.ToLocal(new DateTime(2026, 11, 20, 2, 0, 0, DateTimeKind.Utc));

        Assert.Equal(new DateTime(2026, 11, 20, 10, 0, 0), local);
    }

    [Fact]
    public void Values_without_a_kind_are_read_as_utc()
    {
        var clock = CreateClock(DateTimeOffset.UnixEpoch);

        var local = clock.ToLocal(new DateTime(2026, 6, 18, 7, 26, 35, DateTimeKind.Unspecified));

        Assert.Equal(new DateTime(2026, 6, 18, 15, 26, 35), local);
    }

    [Fact]
    public void Local_wall_clock_times_convert_back_to_utc()
    {
        var clock = CreateClock(DateTimeOffset.UnixEpoch);

        var utc = clock.ToUtc(new DateTime(2026, 11, 20, 8, 0, 0));

        Assert.Equal(new DateTime(2026, 11, 20, 0, 0, 0, DateTimeKind.Utc), utc);
    }

    [Fact]
    public void Local_today_rolls_over_at_local_midnight_not_utc_midnight()
    {
        // 16:30 UTC on Nov 20 is 00:30 on Nov 21 in Manila.
        var clock = CreateClock(new DateTimeOffset(2026, 11, 20, 16, 30, 0, TimeSpan.Zero));

        Assert.Equal(new DateOnly(2026, 11, 21), clock.LocalToday);
        Assert.Equal(new DateTime(2026, 11, 21, 0, 30, 0), clock.LocalNow);
    }

    [Fact]
    public void An_unknown_time_zone_fails_fast()
    {
        Assert.Throws<TimeZoneNotFoundException>(() => CreateClock(DateTimeOffset.UnixEpoch, "Mars/Olympus_Mons"));
    }
}
