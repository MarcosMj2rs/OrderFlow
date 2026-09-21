using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Persistence.Configurations;

public sealed class OrderSagaConfiguration : IEntityTypeConfiguration<OrderSaga>
{
    public void Configure(EntityTypeBuilder<OrderSaga> builder)
    {
        builder.ToTable("OrderSagas");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderId)
               .IsRequired();

        builder.Property(x => x.Status)
               .HasConversion<int>()
               .IsRequired();

        builder.Property(x => x.StartedAt)
               .HasColumnType("datetime2")
               .IsRequired();

        builder.Property(x => x.CompletedAt)
               .HasColumnType("datetime2")
               .IsRequired(false);

        builder.HasIndex(x => x.OrderId)
               .IsUnique()
               .HasDatabaseName("UX_OrderSagas_OrderId");
    }
}