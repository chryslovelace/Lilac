using System.Collections.Generic;
using Lilac.AST;
using Lilac.AST.Expressions;

namespace Lilac.Parser
{
    public interface IParser
    {
        Expression Parse(IEnumerable<Token> tokens);
    }
}