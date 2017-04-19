using System;
using Lilac.AST;
using Lilac.Interpreter;

namespace Lilac.Attributes
{

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class BuiltInFunctionAttribute : Attribute
    {
        public string Name { get; }
        public Type DelegateType { get; }
        public bool IsOperator { get; set; }
        public string Namespace { get; set; }
        public decimal Precedence { get; set; }
        public Association Association { get; set; }

        public OperatorInfo OperatorInfo => IsOperator ? new OperatorInfo(Precedence, Association) : null;

        public BuiltInFunctionAttribute(string name, Type type)
        {
            Name = name;
            DelegateType = type;
        }
    }
}