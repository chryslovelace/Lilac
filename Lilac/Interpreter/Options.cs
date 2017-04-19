using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NDesk.Options;

namespace Lilac.Interpreter
{
    public class Options : IOptions
    {
        public bool RunRepl { get; private set; } = true;
        public bool DisplayHelp { get; private set; }
        public string HelpMessage { get; private set; }
        public TextReader Input { get; private set; } = Console.In;
        public TextWriter Output { get; private set; } = Console.Out;
        public TextWriter Error { get; private set; } = Console.Error;

        private Options(IEnumerable<string> args)
        {
            var options = new OptionSet
            {
                {"f|file=", "The path to the input file. If not present, will run the REPL.", SetInput},
                {"o|output=", "The path to the output file. If not present, defaults to standard out.", SetOutput},
                {"e|error=", "The path to the error file. If not present, defaults to standard error.", SetError},
                {"?|h|help", "Shows this help message.", _ => DisplayHelp = true}
            };

            SetHelpMessage(options);

            try
            {
                var extra = options.Parse(args);
                if (!extra.Any()) return;
                var extras = string.Join(" ", extra);
                throw new OptionException($"Unrecognized options '{extras}'.", extras);
            }
            catch (OptionException e)
            {
                HelpMessage = $"  {e.Message}{Environment.NewLine}{Environment.NewLine}{HelpMessage}";
                DisplayHelp = true;
            }
            catch (Exception e)
            {
                HelpMessage = $"{e.Message}";
                DisplayHelp = true;
            }
        }

        public static IOptions Parse(IEnumerable<string> args)
        {
            return new Options(args);
        }

        private void SetHelpMessage(OptionSet options)
        {
            var sb = new StringBuilder();

            using (var stringWriter = new StringWriter(sb))
            {
                options.WriteOptionDescriptions(stringWriter);
            }

            HelpMessage = sb.ToString();
        }

        private void SetInput(string input)
        {
            RunRepl = false;
            Input = File.OpenText(input);
        }

        private void SetOutput(string output)
        {
            Output = new StreamWriter(File.OpenWrite(output));
        }

        private void SetError(string error)
        {
            Error = new StreamWriter(File.OpenWrite(error));
        }

        public void Dispose()
        {
            Input?.Dispose();
            Output?.Dispose();
            Error?.Dispose();
        }
    }
}