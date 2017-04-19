using System;
using Lilac.Interpreter;

namespace Lilac
{
    public class HelpMessageEntryPoint : IEntryPoint
    {
        private IOptions Options { get; }

        public HelpMessageEntryPoint(IOptions options)
        {
            Options = options;
        }

        public void Run()
        {
            Console.WriteLine(Options.HelpMessage);
        }
    }
}