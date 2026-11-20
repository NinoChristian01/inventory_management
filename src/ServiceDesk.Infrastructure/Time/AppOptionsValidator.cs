using Microsoft.Extensions.Options;
using ServiceDesk.Application.Common;

namespace ServiceDesk.Infrastructure.Time;

/// <summary>
/// Checks at startup that <see cref="AppOptions.TimeZone"/> names a time zone this server knows,
/// so a typo stops the app with a clear message instead of breaking SLA math later.
/// </summary>
public sealed class AppOptionsValidator : IValidateOptions<AppOptions>
{
    public ValidateOptionsResult Validate(string? name, AppOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.TimeZone))
        {
            return ValidateOptionsResult.Fail(
                "App:TimeZone is empty. Set it to an IANA time zone id such as Asia/Manila.");
        }

        if (!TimeZoneInfo.TryFindSystemTimeZoneById(options.TimeZone, out _))
        {
            return ValidateOptionsResult.Fail(
                $"App:TimeZone \"{options.TimeZone}\" isn't a time zone this server knows. Use an IANA id such as Asia/Manila.");
        }

        return ValidateOptionsResult.Success;
    }
}
