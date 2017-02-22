namespace Lilac.AST.Expressions
{
    public class AssignmentExpression : Expression
    {
        public string Name { get; set; }
        public Expression ValueExpression { get; set; }

        public override string ToString()
        {
            return $"set! {Name} = {ValueExpression.ToString()}";
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitAssignment(this);
        }
    }
}