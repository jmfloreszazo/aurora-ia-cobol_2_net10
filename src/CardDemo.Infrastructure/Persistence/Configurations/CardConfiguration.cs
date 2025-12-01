using CardDemo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CardDemo.Infrastructure.Persistence.Configurations;

public class CardConfiguration : IEntityTypeConfiguration<Card>
{
    public void Configure(EntityTypeBuilder<Card> builder)
    {
        builder.ToTable("Cards");

        builder.HasKey(c => c.CardNumber);

        builder.Property(c => c.CardNumber)
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(c => c.AccountId)
            .IsRequired();

        builder.Property(c => c.CardType)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(c => c.EmbossedName)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.ExpirationDate)
            .HasMaxLength(7)
            .IsRequired();

        builder.Property(c => c.ActiveStatus)
            .HasMaxLength(1)
            .IsRequired()
            .HasDefaultValue("Y");

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.CreatedBy)
            .HasMaxLength(50);

        builder.Property(c => c.UpdatedBy)
            .HasMaxLength(50);

        builder.Ignore(c => c.IsActive);
        builder.Ignore(c => c.IsExpired);
        builder.Ignore(c => c.MaskedCardNumber);

        // Relationships
        builder.HasOne(c => c.Account)
            .WithMany(a => a.Cards)
            .HasForeignKey(c => c.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.AccountId);
        builder.HasIndex(c => c.ActiveStatus);
    }
}
