namespace OrderFlow.Infrastructure.Messaging.Exceptions;

public sealed class PermanentMessagingException : MessagingException
{
    public PermanentMessagingException(string message) : base(message) { }
    public PermanentMessagingException(string message, Exception innerException) : base(message, innerException) { }
}
