using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderId)
               .IsRequired();

        builder.Property(x => x.Amount)
               .HasColumnType("decimal(18,2)")
               .IsRequired();

        builder.Property(x => x.CreatedAt)
               .HasColumnType("datetime2")
               .IsRequired();

        builder.HasIndex(x => x.OrderId)
               .IsUnique()
               .HasDatabaseName("UX_Payments_OrderId");
    }
}