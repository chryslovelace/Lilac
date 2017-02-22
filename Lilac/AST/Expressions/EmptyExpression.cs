using System;

namespace Lilac.AST.Expressions
{
    public class EmptyExpression : Expression
    {
        public override string ToString()
        {
            return string.Empty;
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            throw new NotSupportedException();
        }
    }
}