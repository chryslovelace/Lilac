using System.Collections.Generic;

namespace Lilac.AST.Expressions
{
    public class AugmentedIdentifierExpression : Expression
    {
        public override string ToString()
        {
            return $"{string.Join(".", Namespaces)}.{Name}";
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitNamespacedIdentifier(this);
        }

        public List<string> Namespaces { get; set; }
        public string Name { get; set; }
    }
}