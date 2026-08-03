namespace OrderFlow.Infrastructure.Messaging.Exceptions;

public sealed class TransientMessagingException : MessagingException
{
    public TransientMessagingException(string message) : base(message) { }
    public TransientMessagingException(string message, Exception innerException) : base(message, innerException) { }
}
