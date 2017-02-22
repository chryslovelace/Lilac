using System;
using System.Collections.Generic;
using System.Linq;

namespace Lilac.AST.Expressions
{
    public class TopLevelExpression : Expression
    {
        public GroupType GroupType { get; set; }
        public List<Expression> Expressions { get; set; } = new List<Expression>();

        public override string ToString()
        {
            return Expressions.Any()
                ? string.Join(Environment.NewLine, Expressions.Select(e => e.ToString()))
                : "()";
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitTopLevelExpression(this);
        }
    }
}