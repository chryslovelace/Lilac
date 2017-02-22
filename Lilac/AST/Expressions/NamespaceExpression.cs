using System;
using System.Collections.Generic;
using System.Linq;

namespace Lilac.AST.Expressions
{
    public class NamespaceExpression : Expression
    {
        public List<string> Namespaces { get; set; }

        public List<Expression> Expressions { get; set; }
        public GroupType GroupType { get; set; }

        public override string ToString() =>
            $"namespace {string.Join(".", Namespaces)} = {Environment.NewLine} {string.Join(Environment.NewLine, Expressions.Select(e => e.ToString()))}";

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitNamespace(this);
        }
    }
}