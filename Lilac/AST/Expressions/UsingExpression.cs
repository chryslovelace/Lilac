using System.Collections.Generic;

namespace Lilac.AST.Expressions
{
    public class UsingExpression : Expression
    {
        public override string ToString()
        {
            return $"using {string.Join(".", Namespaces)}";
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitUsing(this);
        }

        public List<string> Namespaces { get; set; }
    }
}