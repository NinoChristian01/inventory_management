using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ServiceDesk.Domain.Entities;

namespace ServiceDesk.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<User, Role, long, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketAssignee> TicketAssignees => Set<TicketAssignee>();
    public DbSet<TicketWatcher> TicketWatchers => Set<TicketWatcher>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();
    public DbSet<TicketAttachment> TicketAttachments => Set<TicketAttachment>();
    public DbSet<TicketActivityLog> TicketActivityLogs => Set<TicketActivityLog>();
    public DbSet<TicketSla> TicketSlas => Set<TicketSla>();
    public DbSet<TicketStatus> TicketStatuses => Set<TicketStatus>();
    public DbSet<TicketPriority> TicketPriorities => Set<TicketPriority>();
    public DbSet<TicketCategory> TicketCategories => Set<TicketCategory>();
    public DbSet<SlaPolicy> SlaPolicies => Set<SlaPolicy>();
    public DbSet<BusinessHour> BusinessHours => Set<BusinessHour>();
    public DbSet<Holiday> Holidays => Set<Holiday>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<DeletedRecord> DeletedRecords => Set<DeletedRecord>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();
    public DbSet<TicketNumberSequence> TicketNumberSequences => Set<TicketNumberSequence>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply IEntityTypeConfiguration<T> classes from this assembly when added.
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Default naming: snake_case for tables/columns/keys/indexes unless explicitly configured.
        ApplySnakeCaseNamingConvention(builder);
    }

    private static void ApplySnakeCaseNamingConvention(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entity.GetTableName();
            if (!string.IsNullOrWhiteSpace(tableName))
            {
                entity.SetTableName(ToSnakeCase(tableName));
            }

            foreach (var property in entity.GetProperties())
            {
                var columnName = property.GetColumnName();
                if (!string.IsNullOrWhiteSpace(columnName))
                {
                    property.SetColumnName(ToSnakeCase(columnName));
                }
            }

            foreach (var key in entity.GetKeys())
            {
                var keyName = key.GetName();
                if (!string.IsNullOrWhiteSpace(keyName))
                {
                    key.SetName(ToSnakeCase(keyName));
                }
            }

            foreach (var key in entity.GetForeignKeys())
            {
                var constraintName = key.GetConstraintName();
                if (!string.IsNullOrWhiteSpace(constraintName))
                {
                    key.SetConstraintName(ToSnakeCase(constraintName));
                }
            }

            foreach (var index in entity.GetIndexes())
            {
                var databaseName = index.GetDatabaseName();
                if (!string.IsNullOrWhiteSpace(databaseName))
                {
                    index.SetDatabaseName(ToSnakeCase(databaseName));
                }
            }
        }
    }

    private static string ToSnakeCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;

        var chars = new System.Text.StringBuilder(name.Length + 10);
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c))
            {
                var hasPrev = i > 0;
                var hasNext = i + 1 < name.Length;
                if (hasPrev && (char.IsLower(name[i - 1]) || (hasNext && char.IsLower(name[i + 1]))))
                {
                    chars.Append('_');
                }
                chars.Append(char.ToLowerInvariant(c));
            }
            else
            {
                chars.Append(c);
            }
        }
        return chars.ToString();
    }
}