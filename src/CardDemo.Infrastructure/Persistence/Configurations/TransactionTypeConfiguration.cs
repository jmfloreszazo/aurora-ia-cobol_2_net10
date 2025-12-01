using CardDemo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CardDemo.Infrastructure.Persistence.Configurations;

public class TransactionTypeConfiguration : IEntityTypeConfiguration<TransactionType>
{
    public void Configure(EntityTypeBuilder<TransactionType> builder)
    {
        builder.ToTable("TransactionTypes");

        builder.HasKey(tt => tt.TypeCode);

        builder.Property(tt => tt.TypeCode)
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(tt => tt.TypeDescription)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(tt => tt.CategoryCode)
            .IsRequired();

        builder.Property(tt => tt.CreatedAt)
            .IsRequired();

        builder.Property(tt => tt.CreatedBy)
            .HasMaxLength(50);

        builder.Property(tt => tt.UpdatedBy)
            .HasMaxLength(50);

        // Relationships
        builder.HasOne(tt => tt.Category)
            .WithMany(tc => tc.TransactionTypes)
            .HasForeignKey(tt => tt.CategoryCode)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(tt => tt.CategoryCode);
    }
}
