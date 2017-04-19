using System.Linq;
using System.Reflection;
using Lilac.Attributes;
using Lilac.Values;

namespace Lilac.Interpreter
{
    public class BuiltInProvider : IScopeProvider<Value>
    {
        static BuiltInProvider()
        {
            BuiltInsScope = new Scope<Value>();

            const BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var types = Assembly.GetExecutingAssembly().GetTypes();

            var builtInFuncs =
                from type in types
                from method in type.GetMethods(bindingFlags)
                from attribute in method.GetCustomAttributes<BuiltInFunctionAttribute>()
                select new {attribute, method};

            foreach (var func in builtInFuncs)
            {
                AddBuiltInFunction(func.attribute, func.method);
            }

            var builtInValues =
                from type in types
                from property in type.GetProperties(bindingFlags)
                from attribute in property.GetCustomAttributes<BuiltInValueAttribute>()
                select new {attribute, property};

            foreach (var value in builtInValues)
            {
                AddBuiltInValue(value.attribute, value.property);
            }
        }

        private static void AddBuiltInValue(BuiltInValueAttribute attribute, PropertyInfo property)
        {
            if (string.IsNullOrWhiteSpace(attribute.Namespace))
            {
                BuiltInsScope.BindItem(attribute.Name, (Value)property.GetValue(null));
            }
            else
            {
                var namespaces = attribute.Namespace.Split('.');
                BuiltInsScope.BindNamespacedItem(attribute.Name, (Value)property.GetValue(null), namespaces);
            }
        }

        private static void AddBuiltInFunction(BuiltInFunctionAttribute attribute, MethodInfo method)
        {
            if (string.IsNullOrWhiteSpace(attribute.Namespace))
            {
                BuiltInsScope.BindItem(attribute.Name, new BuiltInFunction(method, attribute.DelegateType), opInfo: attribute.OperatorInfo);
            }
            else
            {
                var namespaces = attribute.Namespace.Split('.');
                BuiltInsScope.BindNamespacedItem(attribute.Name, new BuiltInFunction(method, attribute.DelegateType),
                    namespaces, opInfo: attribute.OperatorInfo);
            }
        }

        private static Scope<Value> BuiltInsScope { get; set; }

        public IScope<Value> GetScope() => BuiltInsScope;

        [BuiltInFunction("list-built-ins", typeof(System.Func<String>))]
        public static String ListBuiltIns()
        {
            return String.Get(BuiltInsScope.ListBindings());
        }
    }
}