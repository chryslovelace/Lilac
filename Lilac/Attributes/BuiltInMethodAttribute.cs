using System;

namespace Lilac.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class BuiltInMethodAttribute : Attribute
    {
        public string Name { get; }
        public Type DelegateType { get; }

        public BuiltInMethodAttribute(string name, Type delegateType)
        {
            Name = name;
            DelegateType = delegateType;
        }
    }
}