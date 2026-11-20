using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceDesk.Domain.Entities;

namespace ServiceDesk.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.Property(x => x.EmployeeNumber).HasMaxLength(50);
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.MiddleName).HasMaxLength(100);
        builder.Property(x => x.Suffix).HasMaxLength(30);
        builder.Property(x => x.FullName).HasMaxLength(300).IsRequired();

        builder.HasIndex(x => x.EmployeeNumber).IsUnique().HasDatabaseName("ux_users_employee_number");
        builder.HasIndex(x => x.LegacyEmployeeId).IsUnique().HasDatabaseName("ux_users_legacy_employee_id");
        builder.HasIndex(x => x.DepartmentId).HasDatabaseName("ix_users_department_id");
        builder.HasIndex(x => x.IsActive).HasDatabaseName("ix_users_is_active");

        builder.HasOne(x => x.Department)
            .WithMany(x => x.DepartmentUsers)
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.UserRoles).WithOne(x => x.User).HasForeignKey(x => x.UserId).IsRequired();
        builder.HasMany(x => x.Claims).WithOne(x => x.User).HasForeignKey(x => x.UserId).IsRequired();
        builder.HasMany(x => x.Logins).WithOne(x => x.User).HasForeignKey(x => x.UserId).IsRequired();
        builder.HasMany(x => x.Tokens).WithOne(x => x.User).HasForeignKey(x => x.UserId).IsRequired();
    }
}