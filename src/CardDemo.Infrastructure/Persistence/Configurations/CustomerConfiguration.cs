using CardDemo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CardDemo.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.CustomerId);

        builder.Property(c => c.CustomerId)
            .ValueGeneratedOnAdd();

        builder.Property(c => c.FirstName)
            .HasMaxLength(25)
            .IsRequired();

        builder.Property(c => c.MiddleName)
            .HasMaxLength(25);

        builder.Property(c => c.LastName)
            .HasMaxLength(25)
            .IsRequired();

        builder.Property(c => c.AddressLine1)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.AddressLine2)
            .HasMaxLength(50);

        builder.Property(c => c.AddressLine3)
            .HasMaxLength(50);

        builder.Property(c => c.StateCode)
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(c => c.CountryCode)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(c => c.ZipCode)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(c => c.PhoneNumber1)
            .HasMaxLength(15)
            .IsRequired();

        builder.Property(c => c.PhoneNumber2)
            .HasMaxLength(15);

        builder.Property(c => c.SSN)
            .HasMaxLength(9)
            .IsRequired();

        builder.Property(c => c.GovernmentId)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.DateOfBirth)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(c => c.FICOScore)
            .IsRequired();

        builder.Property(c => c.EFTAccountId)
            .HasMaxLength(10);

        builder.Property(c => c.PrimaryCardHolder)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.CreatedBy)
            .HasMaxLength(50);

        builder.Property(c => c.UpdatedBy)
            .HasMaxLength(50);

        builder.Ignore(c => c.FullName);
        builder.Ignore(c => c.Age);

        builder.HasIndex(c => c.SSN)
            .IsUnique();

        builder.HasIndex(c => c.GovernmentId);
    }
}
