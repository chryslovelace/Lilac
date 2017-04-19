using System.Collections.Generic;
using Lilac.Utilities;

namespace Lilac.AST.Expressions
{
    public class LambdaExpression : Expression
    {
        public Expression Body { get; set; }
        public List<string> Parameters { get; set; }

        public override string ToString()
        {
            return $"lambda {Parameters.PrettyPrintParameters()} = {Body}";
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitLambda(this);
        }

    }
}