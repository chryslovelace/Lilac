using System.Collections.Generic;
using System.Linq;

namespace Lilac.AST.Expressions
{
    public class ListExpression : Expression
    {
        public List<Expression> Expressions { get; set; }

        public override string ToString()
        {
            return $"[{string.Join("; ", Expressions)}]";
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitList(this);
        }
    }
}