using System;
using System.IO;
using Lilac.AST;
using Lilac.AST.Expressions;
using Lilac.Parser;
using Lilac.Values;

namespace Lilac.Interpreter
{
    public class Interpreter : IInterpreter
    {
        #region Private Properties

        private ILexer Lexer { get; }
        private IParser Parser { get; }
        private IEvaluator Evaluator { get; }
        private IOptions Options { get; }

        #endregion

        public Interpreter(ILexer lexer, IParser parser, IEvaluator evaluator, IOptions options)
        {
            Lexer = lexer;
            Parser = parser;
            Evaluator = evaluator;
            Options = options;
            InjectBuiltIns();
        }

        #region Private Methods

        private void InjectBuiltIns()
        {
            Evaluator.InjectBuiltInValue("print", new BuiltInFunction(new Func<Value, Unit>(Print)));
            Evaluator.InjectBuiltInValue("println", new BuiltInFunction(new Func<Value, Unit>(PrintLn)));
            Evaluator.InjectBuiltInValue("open", new BuiltInFunction(new Func<Values.String, Value>(Open)));
        }

        private Unit Print(Value value)
        {
            Options.Output.Write(value);
            return Unit.Value;
        }

        private Unit PrintLn(Value value)
        {
            Options.Output.WriteLine(value);
            return Unit.Value;
        }

        private Value Open(Values.String filepath)
        {
            return EvaluateProgram(File.OpenText(filepath.ToString()));
        }

        #endregion

        public Value EvaluateProgram(TextReader text)
        {
            try
            {
                var tokens = Lexer.Tokenize(text);
                var expr = Parser.Parse(tokens);
                var value = expr.Accept<Value>(Evaluator);
                return value;
            }
            catch (Exception e)
            {
                Options.Error.WriteLine("Runtime Error: " + e.Message);
                return null;
            }
        }

    }
}