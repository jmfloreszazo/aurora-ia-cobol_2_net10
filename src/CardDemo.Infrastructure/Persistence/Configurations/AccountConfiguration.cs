using CardDemo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CardDemo.Infrastructure.Persistence.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");

        builder.HasKey(a => a.AccountId);

        builder.Property(a => a.AccountId)
            .ValueGeneratedOnAdd();

        builder.Property(a => a.CustomerId)
            .IsRequired();

        builder.Property(a => a.ActiveStatus)
            .HasMaxLength(1)
            .IsRequired()
            .HasDefaultValue("Y");

        builder.Property(a => a.CurrentBalance)
            .HasPrecision(18, 2)
            .IsRequired()
            .HasDefaultValue(0.00m);

        builder.Property(a => a.CreditLimit)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(a => a.CashCreditLimit)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(a => a.OpenDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(a => a.ExpirationDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(a => a.ReissueDate)
            .HasColumnType("date");

        builder.Property(a => a.CurrentCycleCredit)
            .HasPrecision(18, 2)
            .IsRequired()
            .HasDefaultValue(0.00m);

        builder.Property(a => a.CurrentCycleDebit)
            .HasPrecision(18, 2)
            .IsRequired()
            .HasDefaultValue(0.00m);

        builder.Property(a => a.ZipCode)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(a => a.GroupId)
            .HasMaxLength(10);

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.Property(a => a.CreatedBy)
            .HasMaxLength(50);

        builder.Property(a => a.UpdatedBy)
            .HasMaxLength(50);

        builder.Ignore(a => a.IsActive);
        builder.Ignore(a => a.AvailableCredit);
        builder.Ignore(a => a.CreditUtilization);

        // Relationships
        builder.HasOne(a => a.Customer)
            .WithMany(c => c.Accounts)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.CustomerId);
        builder.HasIndex(a => a.ActiveStatus);
    }
}
