using Lilac.Parser;

namespace Lilac.AST.Expressions
{
    public class NumberLiteralExpression : Expression
    {
        public string Value { get; set; }
        public TokenType LiteralType { get; set; }

        public override string ToString()
        {
            return Value;
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitNumberLiteral(this);
        }
    }
}