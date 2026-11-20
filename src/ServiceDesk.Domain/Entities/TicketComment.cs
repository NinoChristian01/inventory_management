namespace ServiceDesk.Domain.Entities;

public class TicketComment
{
    public long Id { get; set; }
    public long TicketId { get; set; }
    public long? AuthorId { get; set; }
    public string Visibility { get; set; } = "public"; // public or internal
    public string CommentType { get; set; } = "reply"; // reply, note, resolution, system
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public Ticket Ticket { get; set; } = null!;
    public User? Author { get; set; }
    public ICollection<TicketAttachment> Attachments { get; set; } = [];
}