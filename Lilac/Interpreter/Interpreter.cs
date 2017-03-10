using System;
using System.IO;
using Lilac.AST;
using Lilac.Attributes;
using Lilac.Parser;
using Lilac.Utilities;
using Lilac.Values;

namespace Lilac.Interpreter
{
    public class Interpreter
    {
        #region Private Properties

        private ILexer Lexer { get; }
        private IParser Parser { get; }
        private IExpressionConsumer<Value> Evaluator { get; }

        #endregion

        public Interpreter(ILexer lexer, IParser parser, IExpressionConsumer<Value> evaluator)
        {
            Lexer = lexer;
            Parser = parser;
            Evaluator = evaluator;
        }

        #region Private Methods

        private Value EvaluateProgram(TextReader text)
        {
            var tokens = Lexer.Tokenize(text);
            var expr = Parser.Parse(tokens);
            var value = Evaluator.Consume(expr);
            return value;
        }

        #endregion

        #region Public Methods

        public void RunRepl()
        {
            while (true)
            {
                Console.Write(">>> ");
                var line = Console.ReadLine();
                if (line == null) break;
                try
                {
                    var value = EvaluateProgram(new StringReader(line));
                    if (!(value is Unit)) Console.WriteLine(value);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Runtime Error: " + e.Message);
                }
            }
        }

        public Value Open(string file)
        {
            var text = File.OpenText(file);
            var value = EvaluateProgram(text);
            return value;
        }

        

        #endregion

        #region Built In Functions

        [BuiltInFunction("quit", typeof(Action))]
        private static void Quit()
        {
            Environment.Exit(0);
        }

        #endregion
    }
}