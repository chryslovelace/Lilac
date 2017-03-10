using System;
using Lilac.AST;
using Lilac.Attributes;
using Lilac.Interpreter;
using Lilac.Parser;
using Lilac.Values;
using SimpleInjector;
using String = Lilac.Values.String;

namespace Lilac
{
    internal static class Program
    {
        private static Interpreter.Interpreter Interpreter { get; set; }
        private static Container Container { get; set; }

        private static void Main(string[] args)
        {
            SetupContainer();

            Interpreter = Container.GetInstance<Interpreter.Interpreter>();
            Interpreter.RunRepl();
        }

        private static void SetupContainer()
        {
            Container = new Container();
            
            Container.Register<ITokenDefiner, TokenDefiner>();
            Container.Register<ILexer, Lexer>();
            Container.Register<IContextDefiner, BuiltInDefiner>();
            Container.Register<IParser, MonadicParserWrapper>();
            Container.Register<IScopeDefiner, BuiltInDefiner>();
            Container.Register<IExpressionConsumer<Value>, Evaluator>();
            Container.Register<IExpressionConsumer<Type>, TypeDeducer>();
            Container.RegisterDecorator<IExpressionConsumer<Value>, TypeDeductionDecorator>();
            Container.Register<Interpreter.Interpreter>();

            Container.Verify();
        }

        [BuiltInFunction("open", typeof(Func<String, Value>))]
        public static Value Open(String file)
        {
            return Interpreter.Open(file.ToString());
        }
    }
}
