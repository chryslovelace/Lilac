namespace Lilac.AST.Expressions
{
    public class MutableBindingExpression : Expression
    {
        public string Name { get; set; }
        public Expression ValueExpression { get; set; }

        public override string ToString()
        {
            return $"let ref {Name} = {ValueExpression.ToString()}";
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitMutableBinding(this);
        }
    }
}