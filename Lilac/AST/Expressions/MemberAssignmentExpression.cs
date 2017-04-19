namespace Lilac.AST.Expressions
{
    public class MemberAssignmentExpression : Expression
    {
        public Expression Target { get; set; }
        public string Member { get; set; }
        public Expression ValueExpression { get; set; }

        public override string ToString()
        {
            return $"set! {Target}.{Member} = {ValueExpression}";
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitMemberAssignment(this);
        }
    }
}