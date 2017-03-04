using System;
using System.Linq;
using System.Reflection;
using Lilac.AST;
using Lilac.AST.Definitions;
using Lilac.Attributes;
using Lilac.Values;
using String = System.String;

namespace Lilac.Interpreter
{
    public class BuiltInDefiner : IContextDefiner, IScopeDefiner
    {

        static BuiltInDefiner()
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
        }

        private static void AddBuiltInValue(BuiltInValueAttribute attribute, PropertyInfo property)
        {
            if (String.IsNullOrWhiteSpace(attribute.Namespace))
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
            if (String.IsNullOrWhiteSpace(attribute.Namespace))
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

        private static Context BuiltInsContext { get; set; }
        private static Scope BuiltInsScope { get; set; }

        public Context GetContext() => BuiltInsContext;
        public Scope GetScope() => BuiltInsScope;

        [BuiltInFunction("list-built-ins", typeof(Func<Values.String>))]
        public static Values.String ListBuiltIns()
        {
            return Values.String.Get(BuiltInsScope.ListBindings());
        }
    }
}