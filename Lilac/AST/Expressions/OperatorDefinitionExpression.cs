using System.Collections.Generic;
using System.Text;
using Lilac.Utilities;

namespace Lilac.AST.Expressions
{
    public class OperatorDefinitionExpression : Expression
    {
        public string Name { get; set; }
        public List<string> Parameters { get; set; }
        public Expression Body { get; set; }
        public decimal Precedence { get; set; }
        public Association Association { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("let operator ").Append(Name);
            sb.Append(" precedence ").Append(Precedence);
            sb.Append(" associates ").Append(Association);
            sb.Append(Parameters.PrettyPrintParameters());
            sb.Append(" = ").Append(Body.ToString());
            return sb.ToString();
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitOperatorDefinition(this);
        }
    }
}