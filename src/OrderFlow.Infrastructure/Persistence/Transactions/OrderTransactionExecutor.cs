
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Application.Abstractions.Persistence;
using OrderFlow.Infrastructure.Persistence.Context;
using System.Data;

namespace OrderFlow.Infrastructure.Persistence.Transactions;

public sealed class OrderTransactionExecutor : IOrderTransactionExecutor
{
    private readonly OrderFlowDbContext _context;
    private readonly IServiceScopeFactory _scopeFactory;

    public OrderTransactionExecutor(OrderFlowDbContext context, IServiceScopeFactory scopeFactory)
    {
        _context = context;
        _scopeFactory = scopeFactory;
    }

    // Legacy: mantido temporariamente durante a migração.
    public async Task ExecuteAsync(Guid orderId, Func<CancellationToken,
                                   Task> operation,
                                   CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        await ExecuteCoreAsync(_context, orderId, operation, cancellationToken);
    }

    // Novo contrato: escopo independente por tentativa.
    public async Task ExecuteAsync(Guid orderId,
                                   Func<IOrderTransactionContext, CancellationToken, Task> operation,
                                   CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        await using var scope = _scopeFactory.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<OrderFlowDbContext>();

        var transactionContext = scope.ServiceProvider.GetRequiredService<IOrderTransactionContext>();

        await ExecuteCoreAsync(context,
                               orderId,
                               ct => operation(transactionContext, ct),
                               cancellationToken);
    }

    private static async Task ExecuteCoreAsync(OrderFlowDbContext context,
                                               Guid orderId,
                                               Func<CancellationToken, Task> operation,
                                               CancellationToken cancellationToken)
    {
        if (context.Database.CurrentTransaction is not null)
        {
            throw new InvalidOperationException("An order transaction is already active in this DbContext.");
        }

        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        try
        {
            var resource = $"OrderFlow:Order:{orderId:N}";

            var connection = (SqlConnection)context.Database.GetDbConnection();

            await using var command = connection.CreateCommand();

            command.Transaction = (SqlTransaction)transaction.GetDbTransaction();

            command.CommandText = """
                DECLARE @result int;

                EXEC @result = sys.sp_getapplock
                    @Resource = @Resource,
                    @LockMode = 'Exclusive',
                    @LockOwner = 'Transaction',
                    @LockTimeout = 15000;

                SELECT @result;
                """;

            command.Parameters.Add(
                new SqlParameter("@Resource", SqlDbType.NVarChar, 255)
                {
                    Value = resource
                });

            var result = await command.ExecuteScalarAsync(cancellationToken);

            var lockResult = Convert.ToInt32(result);

            if (lockResult < 0)
                throw new InvalidOperationException($"Failed to acquire order lock for '{orderId}'. SQL result: {lockResult}.");

            await operation(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            try
            {
                await transaction.RollbackAsync(CancellationToken.None);
            }
            catch
            {
                // Preserva a exceção original.
                // O resultado do commit pode ser desconhecido.
            }

            throw;
        }
    }
}
