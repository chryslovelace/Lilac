using System;

namespace Lilac.Exceptions
{
    public class SyntaxException : Exception
    {
        public SyntaxException() { }
        public SyntaxException(string message) : base(message) { }
        public SyntaxException(string message, Exception innerException) : base(message, innerException) { }
    }
}