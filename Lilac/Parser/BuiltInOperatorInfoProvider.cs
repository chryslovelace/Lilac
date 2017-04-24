using System.Linq;
using System.Reflection;
using Lilac.Attributes;
using Lilac.Interpreter;

namespace Lilac.Parser
{
    public class BuiltInOperatorInfoProvider : IScopeProvider<OperatorInfo>
    {
        private static Scope<OperatorInfo> BuiltInsScope { get; set; }

        static BuiltInOperatorInfoProvider()
        {
            BuiltInsScope = new Scope<OperatorInfo>();
            const BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var types = Assembly.GetExecutingAssembly().GetTypes();

            var builtInOps =
                from type in types
                from method in type.GetMethods(bindingFlags)
                from attribute in method.GetCustomAttributes<BuiltInFunctionAttribute>()
                where attribute.IsOperator
                select attribute;

            foreach (var attribute in builtInOps)
            {
                if (string.IsNullOrWhiteSpace(attribute.Namespace))
                {
                    BuiltInsScope.BindItem(attribute.Name, attribute.OperatorInfo);
                }
                else
                {
                    BuiltInsScope.BindNamespacedItem(attribute.Name, attribute.OperatorInfo,
                        attribute.Namespace.Split('.'));
                }
            }
        }

        public IScope<OperatorInfo> GetScope() => BuiltInsScope;
    }
}