using System;

namespace Lilac.Exceptions
{
    public class TypeResolutionException : Exception
    {
        public TypeResolutionException() { }
        public TypeResolutionException(string message) : base(message) { }
        public TypeResolutionException(string message, Exception innerException) : base(message, innerException) { }
    }
}