using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Persistence.Configurations
{
    public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems");

            builder.HasKey(orderItem => orderItem.Id);

            builder.Property(orderItem => orderItem.Id).ValueGeneratedNever();

            builder.Property<Guid>("OrderId").IsRequired();

            builder.Property(orderItem => orderItem.ProductId).IsRequired();

            builder.Property(orderItem => orderItem.Quantity).IsRequired();

            builder.Property(orderItem => orderItem.UnitPrice).IsRequired().HasPrecision(18, 2);

            builder.Property(orderItem => orderItem.Total).IsRequired().HasPrecision(18, 2);

            builder.Ignore(orderItem => orderItem.DomainEvents);

            builder.HasIndex("OrderId");

            builder.HasIndex(orderItem => orderItem.ProductId);
        }
    }
}
