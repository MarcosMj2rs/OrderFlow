namespace OrderFlow.Infrastructure.Messaging.Exceptions;

public abstract class MessagingException : Exception
{
    protected MessagingException(string message) : base(message) { }

    protected MessagingException(string message, Exception innerException) : base(message, innerException) { }
}
