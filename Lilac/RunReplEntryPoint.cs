using System;
using System.IO;
using Lilac.Interpreter;

namespace Lilac
{
    public class RunReplEntryPoint : IEntryPoint
    {
        private IInterpreter Interpreter { get; }
        private IOptions Options { get; }

        public RunReplEntryPoint(IInterpreter interpreter, IOptions options)
        {
            Interpreter = interpreter;
            Options = options;
        }

        public void Run()
        {
            while (true)
            {
                Console.Write(">>> ");
                var line = Console.ReadLine();
                if (line == null)
                    break;

                try
                {
                    var value = Interpreter.EvaluateProgram(new StringReader(line));
                    Options.Output.WriteLine(value);
                }
                catch (Exception e)
                {
                    Options.Error.WriteLine(e);
                }
            }
        }
    }
}