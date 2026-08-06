namespace OrderFlow.Infrastructure.Persistence.Outbox;

public interface IOutboxEventTypeRegistry
{
    Type Resolve(string eventTypeName);
}
