using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Lilac.AST;
using Lilac.AST.Definitions;
using Lilac.AST.Expressions;
using Lilac.Attributes;
using Lilac.Parser;
using Lilac.Values;
using String = Lilac.Values.String;

namespace Lilac.Interpreter
{
    public static class Interpreter
    {
        #region Private Properties

        private static Scope BuiltInsScope { get; }
        private static Context BuiltInsContext { get; set; }
        private static Lexer Lexer { get; } = new Lexer(TokenDefinition.TokenDefinitions);
        private static ParserState State { get; set; }
        private static Parser<TopLevelExpression> Parser { get; } = Lilac.Parser.Parser.TopLevel();
        private static Evaluator Evaluator { get; }

        #endregion

        #region Type Constructor

        static Interpreter()
        {
            BuiltInsContext = new Context(Enumerable.Empty<Definition>());
            BuiltInsScope = new Scope();
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                foreach (
                    var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    var attributes = method.GetCustomAttributes<BuiltInFunctionAttribute>();
                    foreach (var attribute in attributes)
                    {
                        AddBuiltInFunction(attribute, method);
                    }
                }
                foreach (
                    var property in
                        type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    var attributes = property.GetCustomAttributes<BuiltInValueAttribute>();
                    foreach (var attribute in attributes)
                    {
                        AddBuiltInValue(attribute, property);
                    }
                }
            }
            Evaluator = new Evaluator(BuiltInsScope);
        }

        #endregion

        #region Private Methods

        private static void AddBuiltInValue(BuiltInValueAttribute attribute, PropertyInfo property)
        {
            if (string.IsNullOrWhiteSpace(attribute.Namespace))
            {
                BuiltInsScope.BindValue(attribute.Name, (Value) property.GetValue(null));
                BuiltInsContext = BuiltInsContext.AddDefinition(new Definition(attribute.Name));
            }
            else
            {
                var namespaces = attribute.Namespace.Split('.');
                BuiltInsScope.BindNamespacedValue(attribute.Name, (Value) property.GetValue(null), namespaces);
                BuiltInsContext = BuiltInsContext.AddNamespacedDefinition(namespaces, new Definition(attribute.Name));
            }
        }

        private static void AddBuiltInFunction(BuiltInFunctionAttribute attribute, MethodInfo method)
        {
            var definition = attribute.IsOperator
                ? new OperatorDefinition(attribute.Name, 0, Association.L)
                : new Definition(attribute.Name);
            if (string.IsNullOrWhiteSpace(attribute.Namespace))
            {
                BuiltInsScope.BindValue(attribute.Name, new BuiltInFunction(method, attribute.DelegateType));
                BuiltInsContext = BuiltInsContext.AddDefinition(definition);
            }
            else
            {
                var namespaces = attribute.Namespace.Split('.');
                BuiltInsScope.BindNamespacedValue(attribute.Name, new BuiltInFunction(method, attribute.DelegateType),
                    namespaces);
                BuiltInsContext = BuiltInsContext.AddNamespacedDefinition(namespaces, definition);
            }
        }

        private static Value EvaluateProgram(TextReader text)
        {
            var scope = Evaluator.CurrentScope;
            try
            {
                var tokens = Lexer.Tokenize(text).ToList();
                var state = State?.New(tokens) ?? new ParserState(tokens, BuiltInsContext);
                var expr = Parser.Parse(ref state);
                State = state;
                var value = Evaluator.Evaluate(expr);
                return value;
            }
            catch (Exception)
            {
                Evaluator.ResetScope(scope);
                throw;
            }
        }

        #endregion

        #region Public Methods

        public static void RunRepl()
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

        public static Value Open(string file)
        {
            var text = File.OpenText(file);
            var value = EvaluateProgram(text);
            return value;
        }

        #endregion

        #region Built In Functions

        [BuiltInFunction("open", typeof(Func<String, Value>))]
        private static Value Open(String s)
        {
            return Open(s.ToString());
        }
        
        [BuiltInFunction("reset", typeof(Func<Unit>))]
        public static Unit Reset()
        {
            State = null;
            Evaluator.ResetScope(BuiltInsScope);
            Console.Clear();
            return Unit.Value;
        }

        [BuiltInFunction("quit", typeof(Action))]
        private static void Quit()
        {
            Environment.Exit(0);
        }

        [BuiltInFunction("list-built-ins", typeof(Func<String>))]
        public static String ListBuiltIns()
        {
            return String.Get(BuiltInsScope.ListBindings());
        }

        #endregion
    }
}