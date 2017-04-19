using System.Collections.Generic;
using Lilac.AST.Expressions;

namespace Lilac.Parser
{
    public class MonadicParserWrapper : IParser
    {
        public Expression Parse(IEnumerable<Token> tokens)
        {
            var state = new ParserState(tokens);
            var expr = Parser.TopLevel().Parse(ref state);
            return expr;
        }
    }
}