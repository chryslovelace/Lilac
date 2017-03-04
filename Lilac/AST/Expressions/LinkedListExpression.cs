using System.Collections.Generic;

namespace Lilac.AST.Expressions
{
    public class LinkedListExpression : Expression
    {
        public List<Expression> Expressions { get; set; }

        public override string ToString()
        {
            return $"({string.Join("; ", Expressions)})";
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitLinkedList(this);
        }
    }
}