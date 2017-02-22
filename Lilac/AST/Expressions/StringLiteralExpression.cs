namespace Lilac.AST.Expressions
{
    public class StringLiteralExpression : Expression
    {
        public string Value { get; set; }

        public override string ToString()
        {
            return Value;
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitStringLiteral(this);
        }
    }
}