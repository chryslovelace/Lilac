using System.Collections.Generic;
using Lilac.AST.Expressions;

namespace Lilac.Parser
{
    public class MonadicParserWrapper : IParser
    {
        public Expression Parse(IEnumerable<Token> tokens)
        {
            var expr = Parser.TopLevel().Parse(tokens);
            return expr;
        }
    }
}