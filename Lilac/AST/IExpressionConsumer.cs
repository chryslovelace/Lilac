using Lilac.AST.Expressions;

namespace Lilac.AST
{
    public interface IExpressionConsumer<out T>
    {
        T Consume(Expression expression);
    }
}