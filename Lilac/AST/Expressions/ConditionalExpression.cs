using System.Text;

namespace Lilac.AST.Expressions
{
    public class ConditionalExpression : Expression
    {
        public Expression Condition { get; set; }
        public Expression ThenExpression { get; set; }
        public Expression ElseExpression { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder()
                .Append("if ").Append(Condition)
                .Append(" then ").Append(ThenExpression);

            if (ElseExpression != null)
                sb.Append(" else ").Append(ElseExpression);

            return sb.ToString();
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitConditional(this);
        }
    }
}