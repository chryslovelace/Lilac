using System;
using Lilac.AST.Expressions;
using Lilac.Interpreter;
using Lilac.Parser;
using Lilac.Values;
using SimpleInjector;

namespace Lilac
{
    internal static class Bootstrapper
    {
        public static Container SetupContainer(IOptions options)
        {
            var container = new Container();
            container.Options.DefaultLifestyle = Lifestyle.Singleton;
            container.RegisterSingleton(options);

            if (options.DisplayHelp)
            {
                container.Register<IEntryPoint, HelpMessageEntryPoint>();
            }
            else
            {
                container.Register<ITokenDefiner, TokenDefiner>();
                container.Register<ILexer, Lexer>();
                container.Register<IParser, MonadicParserWrapper>();
                container.Register<IScopeProvider<Value>, BuiltInProvider>();
                container.Register<IEvaluator, Evaluator>();
                container.Register<IInterpreter, Interpreter.Interpreter>();

                if (options.RunRepl)
                {
                    container.Register<IEntryPoint, RunReplEntryPoint>();
                }
                else
                {
                    container.Register<IEntryPoint, InterpretFileEntryPoint>();
                }
            }

            container.Verify();

            return container;
        }
    }
}