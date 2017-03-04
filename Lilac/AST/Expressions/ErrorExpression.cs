using System;
using System.Collections.Generic;
using System.Linq;
using Lilac.Parser;
using Lilac.Values;

namespace Lilac.AST.Expressions
{
    public class ErrorExpression : Expression
    {
        public List<Token> ErrorTokens { get; set; }

        public override string ToString()
        {
            return string.Join(" ", ErrorTokens.Select(t => t.Content));
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitError(this);
        }
    }
}