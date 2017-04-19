using System.Collections.Generic;
using Lilac.Utilities;

namespace Lilac.AST.Expressions
{
    public class FunctionDefinitionExpression : Expression
    {
        public string Name { get; set; }
        public List<string> Parameters { get; set; }
        public Expression Body { get; set; }

        public override string ToString()
        {
            return $"let {Name} {Parameters.PrettyPrintParameters()} = {Body}";
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitFunctionDefinition(this);
        }
    }
}