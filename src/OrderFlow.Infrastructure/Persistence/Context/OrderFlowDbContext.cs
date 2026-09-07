using Microsoft.EntityFrameworkCore;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.Persistence.Inbox;
using OrderFlow.Infrastructure.Persistence.Outbox;

namespace OrderFlow.Infrastructure.Persistence.Context;

public sealed class OrderFlowDbContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
    public DbSet<Payment> Payments => Set<Payment>();
    public OrderFlowDbContext(DbContextOptions<OrderFlowDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderFlowDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
