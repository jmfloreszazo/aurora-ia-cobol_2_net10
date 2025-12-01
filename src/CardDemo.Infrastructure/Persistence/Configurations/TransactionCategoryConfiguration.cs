using CardDemo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CardDemo.Infrastructure.Persistence.Configurations;

public class TransactionCategoryConfiguration : IEntityTypeConfiguration<TransactionCategory>
{
    public void Configure(EntityTypeBuilder<TransactionCategory> builder)
    {
        builder.ToTable("TransactionCategories");

        builder.HasKey(tc => tc.CategoryCode);

        builder.Property(tc => tc.CategoryCode)
            .ValueGeneratedNever();

        builder.Property(tc => tc.CategoryDescription)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(tc => tc.CreatedAt)
            .IsRequired();

        builder.Property(tc => tc.CreatedBy)
            .HasMaxLength(50);

        builder.Property(tc => tc.UpdatedBy)
            .HasMaxLength(50);
    }
}
