namespace Lilac.AST.Expressions
{
    public class BindingExpression : Expression
    {
        public string Name { get; set; }
        public Expression ValueExpression { get; set; }

        public override string ToString()
        {
            return $"let {Name} = {ValueExpression.ToString()}";
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitBinding(this);
        }
    }
}