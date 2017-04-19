using System;
using Lilac.Interpreter;

namespace Lilac
{
    public class InterpretFileEntryPoint : IEntryPoint
    {
        private IInterpreter Interpreter { get; }
        private IOptions Options { get; }

        public InterpretFileEntryPoint(IInterpreter interpreter, IOptions options)
        {
            Interpreter = interpreter;
            Options = options;
        }

        public void Run()
        {
            try
            {
                var value = Interpreter.EvaluateProgram(Options.Input);
                Options.Output.Write(value);
            }
            catch (Exception e)
            {
                Options.Error.Write(e);
            }
        }
    }
}