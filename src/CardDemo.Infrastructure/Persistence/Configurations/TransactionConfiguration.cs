using CardDemo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CardDemo.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");

        builder.HasKey(t => t.TransactionId);

        builder.Property(t => t.TransactionId)
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(t => t.AccountId)
            .IsRequired();

        builder.Property(t => t.CardNumber)
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(t => t.TransactionType)
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(t => t.CategoryCode)
            .IsRequired();

        builder.Property(t => t.TransactionSource)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(t => t.MerchantId)
            .HasMaxLength(9);

        builder.Property(t => t.MerchantName)
            .HasMaxLength(50);

        builder.Property(t => t.MerchantCity)
            .HasMaxLength(50);

        builder.Property(t => t.MerchantZip)
            .HasMaxLength(10);

        builder.Property(t => t.OrigTransactionId)
            .HasMaxLength(16);

        builder.Property(t => t.TransactionDate)
            .IsRequired();

        builder.Property(t => t.ProcessedFlag)
            .HasMaxLength(1)
            .IsRequired()
            .HasDefaultValue("N");

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.CreatedBy)
            .HasMaxLength(50);

        builder.Property(t => t.UpdatedBy)
            .HasMaxLength(50);

        builder.Ignore(t => t.IsProcessed);
        builder.Ignore(t => t.IsDebit);
        builder.Ignore(t => t.IsCredit);
        builder.Ignore(t => t.IsReversal);

        // Relationships
        builder.HasOne(t => t.Account)
            .WithMany(a => a.Transactions)
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Card)
            .WithMany(c => c.Transactions)
            .HasForeignKey(t => t.CardNumber)
            .HasPrincipalKey(c => c.CardNumber)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.TransactionTypeNavigation)
            .WithMany(tt => tt.Transactions)
            .HasForeignKey(t => t.TransactionType)
            .HasPrincipalKey(tt => tt.TypeCode)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Category)
            .WithMany(tc => tc.Transactions)
            .HasForeignKey(t => t.CategoryCode)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.AccountId);
        builder.HasIndex(t => t.CardNumber);
        builder.HasIndex(t => t.TransactionDate);
        builder.HasIndex(t => t.ProcessedFlag);
        builder.HasIndex(t => new { t.AccountId, t.TransactionDate });
    }
}
