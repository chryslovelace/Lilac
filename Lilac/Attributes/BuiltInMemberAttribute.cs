using System;

namespace Lilac.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class BuiltInMemberAttribute : Attribute
    {
        public string Name { get; }
        public bool GetOnly { get; set; }

        public BuiltInMemberAttribute(string name)
        {
            Name = name;
        }
    }
}