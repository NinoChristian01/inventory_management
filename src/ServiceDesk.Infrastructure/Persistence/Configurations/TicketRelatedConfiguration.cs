using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceDesk.Domain.Entities;

namespace ServiceDesk.Infrastructure.Persistence.Configurations;

public class TicketAssigneeConfiguration : IEntityTypeConfiguration<TicketAssignee>
{
    public void Configure(EntityTypeBuilder<TicketAssignee> builder)
    {
        builder.ToTable("ticket_assignees");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.TicketId, x.UserId, x.UnassignedAtUtc }).HasDatabaseName("ix_ticket_assignees_ticket_user_unassigned");
        builder.HasIndex(x => new { x.TicketId, x.IsPrimary, x.UnassignedAtUtc }).HasDatabaseName("ix_ticket_assignees_ticket_primary_unassigned");

        builder.HasOne(x => x.Ticket).WithMany(x => x.Assignees).HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class TicketWatcherConfiguration : IEntityTypeConfiguration<TicketWatcher>
{
    public void Configure(EntityTypeBuilder<TicketWatcher> builder)
    {
        builder.ToTable("ticket_watchers");
        builder.HasKey(x => new { x.TicketId, x.UserId });

        builder.HasOne(x => x.Ticket).WithMany(x => x.Watchers).HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class TicketCommentConfiguration : IEntityTypeConfiguration<TicketComment>
{
    public void Configure(EntityTypeBuilder<TicketComment> builder)
    {
        builder.ToTable("ticket_comments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Visibility).HasMaxLength(20).IsRequired();
        builder.Property(x => x.CommentType).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Body).HasColumnType("longtext").IsRequired();

        builder.HasIndex(x => new { x.TicketId, x.CreatedAtUtc }).HasDatabaseName("ix_ticket_comments_ticket_created");

        builder.HasOne(x => x.Ticket).WithMany(x => x.Comments).HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Author).WithMany().HasForeignKey(x => x.AuthorId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class TicketAttachmentConfiguration : IEntityTypeConfiguration<TicketAttachment>
{
    public void Configure(EntityTypeBuilder<TicketAttachment> builder)
    {
        builder.ToTable("ticket_attachments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OriginalFileName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.StoredFileName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Sha256).HasMaxLength(64).IsRequired();

        builder.HasIndex(x => x.TicketId).HasDatabaseName("ix_ticket_attachments_ticket_id");
        builder.HasIndex(x => x.CommentId).HasDatabaseName("ix_ticket_attachments_comment_id");

        builder.HasOne(x => x.Ticket).WithMany(x => x.Attachments).HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Comment).WithMany(x => x.Attachments).HasForeignKey(x => x.CommentId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.UploadedBy).WithMany().HasForeignKey(x => x.UploadedById).OnDelete(DeleteBehavior.SetNull);
    }
}

public class TicketActivityLogConfiguration : IEntityTypeConfiguration<TicketActivityLog>
{
    public void Configure(EntityTypeBuilder<TicketActivityLog> builder)
    {
        builder.ToTable("ticket_activity_logs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ActorName).HasMaxLength(300);
        builder.Property(x => x.EventType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.FieldName).HasMaxLength(100);
        builder.Property(x => x.OldValue).HasColumnType("longtext");
        builder.Property(x => x.NewValue).HasColumnType("longtext");
        builder.Property(x => x.MetadataJson).HasColumnType("json");

        builder.HasIndex(x => new { x.TicketId, x.CreatedAtUtc }).HasDatabaseName("ix_ticket_activity_logs_ticket_created");
        builder.HasIndex(x => x.EventType).HasDatabaseName("ix_ticket_activity_logs_event_type");

        builder.HasOne(x => x.Ticket).WithMany(x => x.ActivityLogs).HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Actor).WithMany().HasForeignKey(x => x.ActorId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class TicketSlaConfiguration : IEntityTypeConfiguration<TicketSla>
{
    public void Configure(EntityTypeBuilder<TicketSla> builder)
    {
        builder.ToTable("ticket_slas");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Metric).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Stage).HasMaxLength(20).IsRequired();

        builder.HasIndex(x => new { x.TicketId, x.Metric }).HasDatabaseName("ix_ticket_slas_ticket_metric");
        builder.HasIndex(x => x.DueAtUtc).HasDatabaseName("ix_ticket_slas_due_at_utc");
        builder.HasIndex(x => x.Stage).HasDatabaseName("ix_ticket_slas_stage");

        builder.HasOne(x => x.Ticket).WithMany(x => x.Slas).HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.SlaPolicy).WithMany().HasForeignKey(x => x.SlaPolicyId).OnDelete(DeleteBehavior.Restrict);
    }
}