namespace Lilac.AST.Expressions
{
    public class OperatorCallExpression : Expression
    {
        public string Name { get; set; }
        public Expression Lhs { get; set; }
        public Expression Rhs { get; set; }

        public override string ToString()
        {
            return $"({Lhs} {Name} {Rhs})";
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitOperatorCall(this);
        }
    }
}