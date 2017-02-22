using System;
using Lilac.Parser;

namespace Lilac.Exceptions
{
    public class ParseException : Exception
    {
        public ParserState State { get; set; }

        public ParseException() { }
        public ParseException(string message) : base(message) { }
        public ParseException(string message, ParserState state) : base(message)
        {
            State = state;
        }
        public ParseException(string message, Exception innerException) : base(message, innerException) { }
    }
}