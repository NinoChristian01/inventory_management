namespace ServiceDesk.Domain.Entities;

public class AppSetting
{
    public string SettingKey { get; set; } = string.Empty;
    public string? SettingValue { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}