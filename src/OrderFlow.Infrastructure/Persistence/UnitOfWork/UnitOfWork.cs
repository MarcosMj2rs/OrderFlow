using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Infrastructure.Persistence.Context;

namespace OrderFlow.Infrastructure.Persistence.UnitOfWork
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly OrderFlowDbContext _context;

        public UnitOfWork(OrderFlowDbContext context)
        {
            _context = context;
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
