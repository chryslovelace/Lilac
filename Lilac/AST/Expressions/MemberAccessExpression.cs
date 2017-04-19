namespace Lilac.AST.Expressions
{
    public class MemberAccessExpression : Expression
    {
        public Expression Target { get; set; }
        public string Member { get; set; }

        public override string ToString() => $"{Target}.{Member}";

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitMemberAccess(this);
        }
    }
}