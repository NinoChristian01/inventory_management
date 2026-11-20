namespace ServiceDesk.Web.Logging;

/// <summary>Settings for the rolling log files, from the "LogFiles" section of appsettings.json.</summary>
public sealed class LogFileOptions
{
    public const string SectionName = "LogFiles";

    /// <summary>Turn file logging off, for example in integration tests.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Folder for log files. Relative paths are resolved from the content root.</summary>
    public string Directory { get; set; } = "logs";

    /// <summary>Days of general logs to keep. Error logs are kept three times as long.</summary>
    public int RetainedDays { get; set; } = 30;
}
