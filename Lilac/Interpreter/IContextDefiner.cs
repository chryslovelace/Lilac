using Lilac.AST;

namespace Lilac.Interpreter
{
    public interface IContextDefiner
    {
        Context GetContext();
    }
}