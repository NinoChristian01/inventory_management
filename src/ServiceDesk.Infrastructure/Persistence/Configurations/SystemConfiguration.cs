using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceDesk.Domain.Entities;

namespace ServiceDesk.Infrastructure.Persistence.Configurations;

public class SlaPolicyConfiguration : IEntityTypeConfiguration<SlaPolicy>
{
    public void Configure(EntityTypeBuilder<SlaPolicy> builder)
    {
        builder.ToTable("sla_policies");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.PriorityId, x.CategoryId, x.IsActive }).HasDatabaseName("ix_sla_policies_priority_category_active");

        builder.HasOne(x => x.Priority).WithMany().HasForeignKey(x => x.PriorityId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class BusinessHourConfiguration : IEntityTypeConfiguration<BusinessHour>
{
    public void Configure(EntityTypeBuilder<BusinessHour> builder)
    {
        builder.ToTable("business_hours");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.DayOfWeek).HasDatabaseName("ix_business_hours_day_of_week");
    }
}

public class HolidayConfiguration : IEntityTypeConfiguration<Holiday>
{
    public void Configure(EntityTypeBuilder<Holiday> builder)
    {
        builder.ToTable("holidays");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.HasIndex(x => x.HolidayDate).IsUnique().HasDatabaseName("ux_holidays_holiday_date");
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Body).HasColumnType("longtext");
        builder.Property(x => x.LinkUrl).HasMaxLength(500);

        builder.HasIndex(x => new { x.RecipientId, x.IsRead, x.CreatedAtUtc }).HasDatabaseName("ix_notifications_recipient_read_created");

        builder.HasOne(x => x.Recipient).WithMany().HasForeignKey(x => x.RecipientId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Ticket).WithMany().HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ActorName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.TableName).HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.OldValues).HasColumnType("json");
        builder.Property(x => x.NewValues).HasColumnType("json");

        builder.HasIndex(x => x.CreatedAtUtc).HasDatabaseName("ix_audit_logs_created_at_utc");
        builder.HasIndex(x => new { x.TableName, x.RecordId }).HasDatabaseName("ix_audit_logs_table_record");
    }
}

public class DeletedRecordConfiguration : IEntityTypeConfiguration<DeletedRecord>
{
    public void Configure(EntityTypeBuilder<DeletedRecord> builder)
    {
        builder.ToTable("deleted_records");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EntityName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(255);
        builder.Property(x => x.Snapshot).HasColumnType("json").IsRequired();
        builder.Property(x => x.DeletedByName).HasMaxLength(300);

        builder.HasIndex(x => new { x.EntityName, x.RestoredAtUtc }).HasDatabaseName("ix_deleted_records_entity_restored");
    }
}

public class AppSettingConfiguration : IEntityTypeConfiguration<AppSetting>
{
    public void Configure(EntityTypeBuilder<AppSetting> builder)
    {
        builder.ToTable("app_settings");
        builder.HasKey(x => x.SettingKey);
        builder.Property(x => x.SettingKey).HasMaxLength(100);
        builder.Property(x => x.SettingValue).HasColumnType("longtext");
    }
}

public class TicketNumberSequenceConfiguration : IEntityTypeConfiguration<TicketNumberSequence>
{
    public void Configure(EntityTypeBuilder<TicketNumberSequence> builder)
    {
        builder.ToTable("ticket_number_sequences");
        builder.HasKey(x => x.ScopeKey);
        builder.Property(x => x.ScopeKey).HasMaxLength(50);
    }
}