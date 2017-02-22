using System;

namespace Lilac.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class BuiltInValueAttribute : Attribute
    {
        public string Name { get; }
        public string Namespace { get; set; }

        public BuiltInValueAttribute(string name)
        {
            Name = name;
        }
    }
}