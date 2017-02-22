using System;

namespace Lilac.Exceptions
{
    public class NumericTypeException : Exception
    {
        public NumericTypeException() { }
        public NumericTypeException(string message) : base(message) { }
        public NumericTypeException(string message, Exception innerException) : base(message, innerException) { }
    }
}