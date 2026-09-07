namespace OrderFlow.Application.Exceptions;

public sealed class UniqueConstraintException : Exception
{
    public UniqueConstraintException(string message, Exception innerException) : base(message, innerException) { }
}