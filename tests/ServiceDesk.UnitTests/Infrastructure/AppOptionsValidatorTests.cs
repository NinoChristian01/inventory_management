using ServiceDesk.Application.Common;
using ServiceDesk.Infrastructure.Time;

namespace ServiceDesk.UnitTests.Infrastructure;

public sealed class AppOptionsValidatorTests
{
    private readonly AppOptionsValidator _validator = new();

    [Fact]
    public void Manila_is_accepted()
    {
        var result = _validator.Validate(null, new AppOptions { TimeZone = "Asia/Manila", OrganizationName = "MIS" });

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void An_unknown_time_zone_is_rejected_with_a_fix()
    {
        var result = _validator.Validate(null, new AppOptions { TimeZone = "Asia/Manilla", OrganizationName = "MIS" });

        Assert.True(result.Failed);
        Assert.Contains("isn't a time zone this server knows", result.FailureMessage, StringComparison.Ordinal);
        Assert.Contains("Asia/Manila", result.FailureMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void An_empty_time_zone_is_rejected()
    {
        var result = _validator.Validate(null, new AppOptions { TimeZone = " ", OrganizationName = "MIS" });

        Assert.True(result.Failed);
        Assert.Contains("App:TimeZone is empty", result.FailureMessage, StringComparison.Ordinal);
    }
}
