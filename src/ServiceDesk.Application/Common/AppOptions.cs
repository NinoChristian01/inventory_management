using System.ComponentModel.DataAnnotations;

namespace ServiceDesk.Application.Common;

/// <summary>
/// App-wide settings from the "App" section of appsettings.json. Validated at startup, so a
/// typo in the time zone stops the app with a clear message instead of failing later.
/// </summary>
public sealed class AppOptions
{
    public const string SectionName = "App";

    /// <summary>Product name shown in the sidebar and the browser tab.</summary>
    [Required, StringLength(60)]
    public string Name { get; set; } = "ServiceDesk";

    /// <summary>The office that runs the service desk, shown under the product name.</summary>
    [Required, StringLength(150)]
    public string OrganizationName { get; set; } = string.Empty;

    /// <summary>IANA time zone id used for display, business hours and "today".</summary>
    [Required]
    public string TimeZone { get; set; } = "Asia/Manila";
}
