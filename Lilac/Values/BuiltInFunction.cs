using System;
using System.Reflection;

namespace Lilac.Values
{
    public class BuiltInFunction : Value
    {
        public Delegate Method { get; }
        public int ParameterCount { get; }
        
        public BuiltInFunction(MethodInfo methodInfo, Type delegateType, object target = null)
        {
            Method = methodInfo.CreateDelegate(delegateType, target);
            ParameterCount = methodInfo.GetParameters().Length;
        }

        public override string ToString()
        {
            return $"<#builtin {Method.Method}>";
        }

        public override bool IsCallable()
        {
            return true;
        }
    }
}