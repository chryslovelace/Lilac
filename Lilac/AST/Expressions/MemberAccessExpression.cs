namespace Lilac.AST.Expressions
{
    public class MemberAccessExpression : Expression
    {
        public Expression Target { get; set; }
        public string Member { get; set; }

        public override string ToString() => $"{Target.ToString()}.{Member}";

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitMemberAccess(this);
        }

        public override Expression ResolvePrecedence()
        {
            var lhsFunc = Target as FunctionCallExpression;
            if (lhsFunc != null)
            {
                return new FunctionCallExpression
                {
                    Function = lhsFunc.Function,
                    Argument = new MemberAccessExpression
                    {
                        Target = lhsFunc.Argument,
                        Member = Member
                    }.ResolvePrecedence()
                };
            }
            var lhsOp = Target as OperatorCallExpression;
            if (lhsOp != null)
            {
                return new OperatorCallExpression
                {
                    Lhs = lhsOp.Lhs,
                    Name = lhsOp.Name,
                    Rhs = new MemberAccessExpression
                    {
                        Target = lhsOp.Rhs,
                        Member = Member
                    }.ResolvePrecedence()
                };
            }

            return this;
        }
    }
}