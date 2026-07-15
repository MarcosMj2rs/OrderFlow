using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Persistence.Configurations
{
    public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.HasKey(order => order.Id);

            builder.Property(order => order.Id).ValueGeneratedNever();

            builder.Property(order => order.CustomerId).IsRequired();

            builder.Property(order => order.CreatedAt).IsRequired().HasColumnType("datetime2");

            builder.Property(order => order.Status).IsRequired().HasConversion<int>();

            builder.Property(order => order.TotalAmount).IsRequired().HasPrecision(18, 2);

            builder.Ignore(order => order.DomainEvents);

            builder.HasMany(order => order.Items)
                   .WithOne()
                   .HasForeignKey("OrderId")
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(order => order.Items)
                   .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
