using System;
using System.Collections.Generic;
using System.Text;

namespace OrderFlow.Domain.Exceptions
{
    public sealed class DomainException : Exception
    {
        public DomainException(string message) : base(message) { }
    }
}
