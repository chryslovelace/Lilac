using System.Collections.Generic;

namespace Lilac.AST.Expressions
{
    public class GroupExpression : Expression
    {
        public GroupType GroupType { get; set; }
        public List<Expression> Expressions { get; set; } = new List<Expression>();

        public override string ToString()
        {
            return $"({string.Join("; ", Expressions)})";
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitGroup(this);
        }
    }
}