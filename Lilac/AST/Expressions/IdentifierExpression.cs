namespace Lilac.AST.Expressions
{
    public class IdentifierExpression : Expression
    {
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitIdentifier(this);
        }
    }
}