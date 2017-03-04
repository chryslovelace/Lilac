using System.Collections.Generic;
using Lilac.AST;
using Lilac.AST.Expressions;
using Lilac.Interpreter;

namespace Lilac.Parser
{
    public class MonadicParserWrapper : IParser
    {
        private Context Context { get; set; }

        public MonadicParserWrapper(IContextDefiner contextDefiner)
        {
            Context = contextDefiner.GetContext();
        }

        public Expression Parse(IEnumerable<Token> tokens)
        {
            var state = new ParserState(tokens, Context);
            var expr = Parser.TopLevel().Parse(ref state);
            Context = state.Context;
            return expr;
        }
    }
}