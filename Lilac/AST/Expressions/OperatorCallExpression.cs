namespace Lilac.AST.Expressions
{
    public class OperatorCallExpression : Expression
    {
        public string Name { get; set; }
        public Expression Lhs { get; set; }
        public Expression Rhs { get; set; }

        public override string ToString()
        {
            return $"({Lhs.ToString()} {Name} {Rhs.ToString()})";
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitOperatorCall(this);
        }

        public override Expression ResolvePrecedence()
        {
            var rhsOp = Rhs as OperatorCallExpression;
            if (rhsOp != null)
            {
                return new OperatorCallExpression
                {
                    Lhs = new OperatorCallExpression
                    {
                        Lhs = Lhs,
                        Name = Name,
                        Rhs = rhsOp.Lhs
                    }.ResolvePrecedence(),
                    Name = rhsOp.Name,
                    Rhs = rhsOp.Rhs
                };
            }

            return this;
        }
    }
}