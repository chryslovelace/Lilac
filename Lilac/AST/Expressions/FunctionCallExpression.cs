using System.Collections.Generic;

namespace Lilac.AST.Expressions
{
    public class FunctionCallExpression : Expression
    {
        public Expression Function { get; set; }
        public List<Expression> Arguments { get; set; }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitFunctionCall(this);
        }
    }
}