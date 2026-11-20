using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceDesk.Domain.Entities;

namespace ServiceDesk.Infrastructure.Persistence.Configurations;

public class TicketStatusConfiguration : IEntityTypeConfiguration<TicketStatus>
{
    public void Configure(EntityTypeBuilder<TicketStatus> builder)
    {
        builder.ToTable("ticket_statuses");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("ux_ticket_statuses_code");
    }
}

public class TicketPriorityConfiguration : IEntityTypeConfiguration<TicketPriority>
{
    public void Configure(EntityTypeBuilder<TicketPriority> builder)
    {
        builder.ToTable("ticket_priorities");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Color).HasMaxLength(20);
        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("ux_ticket_priorities_code");
    }
}

public class TicketCategoryConfiguration : IEntityTypeConfiguration<TicketCategory>
{
    public void Configure(EntityTypeBuilder<TicketCategory> builder)
    {
        builder.ToTable("ticket_categories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();

        builder.HasIndex(x => x.ParentId).HasDatabaseName("ix_ticket_categories_parent_id");
        builder.HasIndex(x => x.DefaultTeamId).HasDatabaseName("ix_ticket_categories_default_team_id");
        builder.HasIndex(x => x.IsActive).HasDatabaseName("ix_ticket_categories_is_active");

        builder.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DefaultTeam)
            .WithMany()
            .HasForeignKey(x => x.DefaultTeamId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("tickets");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TicketNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.RequesterNameSnapshot).HasMaxLength(300);
        builder.Property(x => x.Subject).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Description).HasColumnType("longtext");
        builder.Property(x => x.ResolutionCode).HasMaxLength(50);
        builder.Property(x => x.ResolutionSummary).HasColumnType("longtext");
        builder.Property(x => x.CloseReason).HasMaxLength(500);
        builder.Property(x => x.CancelReason).HasMaxLength(500);
        builder.Property(x => x.SatisfactionComment).HasColumnType("longtext");
        builder.Property(x => x.LegacyNotedByName).HasMaxLength(200);
        builder.Property(x => x.LegacyApprovedByName).HasMaxLength(200);
        builder.Property(x => x.LegacyTurnaroundUnit).HasMaxLength(20);
        builder.Property(x => x.Version).IsConcurrencyToken();

        builder.HasIndex(x => x.TicketNumber).IsUnique().HasDatabaseName("ux_tickets_ticket_number");
        builder.HasIndex(x => x.LegacyRequestId).IsUnique().HasDatabaseName("ux_tickets_legacy_request_id");
        builder.HasIndex(x => x.RequesterId).HasDatabaseName("ix_tickets_requester_id");
        builder.HasIndex(x => x.RequesterDepartmentId).HasDatabaseName("ix_tickets_requester_department_id");
        builder.HasIndex(x => x.StatusId).HasDatabaseName("ix_tickets_status_id");
        builder.HasIndex(x => x.PriorityId).HasDatabaseName("ix_tickets_priority_id");
        builder.HasIndex(x => x.CategoryId).HasDatabaseName("ix_tickets_category_id");
        builder.HasIndex(x => x.AssignedTeamId).HasDatabaseName("ix_tickets_assigned_team_id");
        builder.HasIndex(x => x.PrimaryAssigneeId).HasDatabaseName("ix_tickets_primary_assignee_id");
        builder.HasIndex(x => x.CreatedAtUtc).HasDatabaseName("ix_tickets_created_at_utc");
        builder.HasIndex(x => new { x.StatusId, x.PriorityId, x.CreatedAtUtc }).HasDatabaseName("ix_tickets_status_priority_created");

        builder.HasOne(x => x.Requester).WithMany().HasForeignKey(x => x.RequesterId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.RequesterDepartment).WithMany().HasForeignKey(x => x.RequesterDepartmentId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.Status).WithMany().HasForeignKey(x => x.StatusId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Priority).WithMany().HasForeignKey(x => x.PriorityId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.AssignedTeam).WithMany().HasForeignKey(x => x.AssignedTeamId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.PrimaryAssignee).WithMany().HasForeignKey(x => x.PrimaryAssigneeId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.ResolvedBy).WithMany().HasForeignKey(x => x.ResolvedById).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.ClosedBy).WithMany().HasForeignKey(x => x.ClosedById).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.CanceledBy).WithMany().HasForeignKey(x => x.CanceledById).OnDelete(DeleteBehavior.SetNull);
    }
}