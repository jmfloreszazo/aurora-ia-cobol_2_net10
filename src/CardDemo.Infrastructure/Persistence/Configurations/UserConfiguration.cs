using CardDemo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CardDemo.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.UserId);

        builder.Property(u => u.UserId)
            .HasMaxLength(8)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.FirstName)
            .HasMaxLength(25)
            .IsRequired();

        builder.Property(u => u.LastName)
            .HasMaxLength(25)
            .IsRequired();

        builder.Property(u => u.UserType)
            .HasConversion<string>()
            .HasMaxLength(5)
            .IsRequired();

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.IsLocked)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.FailedLoginAttempts)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.CreatedBy)
            .HasMaxLength(50);

        builder.Property(u => u.UpdatedBy)
            .HasMaxLength(50);

        builder.Ignore(u => u.FullName);

        builder.HasIndex(u => u.UserId)
            .IsUnique();
    }
}
