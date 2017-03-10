using System;

namespace Lilac.AST.Expressions
{
    public abstract class Expression
    {
        public Type ExpressionType { get; set; }

        public abstract T Accept<T>(IExpressionVisitor<T> visitor);

        public virtual Expression ResolvePrecedence() => this;
    }
}