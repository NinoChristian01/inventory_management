namespace ServiceDesk.Domain.Entities;

public class DeletedRecord
{
    public long Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public long RecordId { get; set; }
    public string? DisplayName { get; set; }
    public string Snapshot { get; set; } = "{}"; // JSON
    public long? DeletedById { get; set; }
    public string? DeletedByName { get; set; }
    public DateTime DeletedAtUtc { get; set; }
    public DateTime? RestoredAtUtc { get; set; }
}