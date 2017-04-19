using System.IO;
using Lilac.Values;

namespace Lilac.Interpreter
{
    public interface IInterpreter
    {
        Value EvaluateProgram(TextReader text);
    }
}