namespace Lilac.Interpreter
{
    public interface IScopeProvider<T>
    {
        IScope<T> GetScope();
    }
}