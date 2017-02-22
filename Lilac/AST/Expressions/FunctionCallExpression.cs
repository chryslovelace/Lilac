namespace Lilac.AST.Expressions
{
    public class FunctionCallExpression : Expression
    {
        public Expression Function { get; set; }
        public Expression Argument { get; set; }

        public override string ToString()
        {
            return $"({Function.ToString()} {Argument.ToString()})";
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitFunctionCall(this);
        }

        public override Expression ResolvePrecedence()
        {
            var rhsFunc = Argument as FunctionCallExpression;
            if (rhsFunc != null)
            {
                return new FunctionCallExpression
                {
                    Function = new FunctionCallExpression
                    {
                        Function = Function,
                        Argument = rhsFunc.Function
                    }.ResolvePrecedence(),
                    Argument = rhsFunc.Argument
                };
            }
            var rhsOp = Argument as OperatorCallExpression;
            if (rhsOp != null)
            {
                return new OperatorCallExpression
                {
                    Lhs = new FunctionCallExpression
                    {
                        Function = Function,
                        Argument = rhsOp.Lhs
                    }.ResolvePrecedence(),
                    Name = rhsOp.Name,
                    Rhs = rhsOp.Rhs
                };
            }

            return this;
        }
    }
}