using System;
using System.IO;

namespace Lilac.Interpreter
{
    public interface IOptions : IDisposable
    {
        bool RunRepl { get; }
        bool DisplayHelp { get; }
        string HelpMessage { get; }
        TextReader Input { get; }
        TextWriter Output { get; }
        TextWriter Error { get; }
    }
}