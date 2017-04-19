namespace Lilac.AST.Expressions
{
    public class ApplicationExpression : Expression
    {
        public Expression Function { get; set; }
        public Expression Argument { get; set; }

        public override string ToString()
        {
            return $"({Function} {Argument})";
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitApplication(this);
        }
    }
}