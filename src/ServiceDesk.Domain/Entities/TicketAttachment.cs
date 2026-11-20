namespace ServiceDesk.Domain.Entities;

public class TicketAttachment
{
    public long Id { get; set; }
    public long TicketId { get; set; }
    public long? CommentId { get; set; }
    public long? UploadedById { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty; // random name
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string Sha256 { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public Ticket Ticket { get; set; } = null!;
    public TicketComment? Comment { get; set; }
    public User? UploadedBy { get; set; }
}