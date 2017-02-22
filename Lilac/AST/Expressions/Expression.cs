namespace Lilac.AST.Expressions
{
    public abstract class Expression
    {
        public abstract T Accept<T>(IExpressionVisitor<T> visitor);

        public virtual Expression ResolvePrecedence() => this;
    }
}