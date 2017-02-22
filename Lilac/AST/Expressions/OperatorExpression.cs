namespace Lilac.AST.Expressions
{
    public class OperatorExpression : Expression
    {
        public string Name { get; set; }
        public override string ToString() => Name;

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitOperator(this);
        }
    }
}