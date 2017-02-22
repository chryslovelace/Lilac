using System;

namespace Lilac.Attributes
{

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class BuiltInFunctionAttribute : Attribute
    {
        public string Name { get; }
        public Type DelegateType { get; }
        public bool IsOperator { get; set; }
        public string Namespace { get; set; }

        public BuiltInFunctionAttribute(string name, Type type)
        {
            Name = name;
            DelegateType = type;
        }
    }
}