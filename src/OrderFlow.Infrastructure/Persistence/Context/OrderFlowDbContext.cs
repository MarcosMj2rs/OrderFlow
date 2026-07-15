using Microsoft.EntityFrameworkCore;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Persistence.Context
{
    public sealed class OrderFlowDbContext : DbContext
    {
        public DbSet<Order> Orders => Set<Order>();
        public OrderFlowDbContext(DbContextOptions<OrderFlowDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderFlowDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
