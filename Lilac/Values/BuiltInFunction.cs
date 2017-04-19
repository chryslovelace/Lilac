using System;
using System.Reflection;

namespace Lilac.Values
{
    public class BuiltInFunction : Value
    {
        private Func<Value, Unit> print;

        public Delegate Method { get; }
        public int ParameterCount { get; }
        
        public BuiltInFunction(MethodInfo methodInfo, Type delegateType, object target = null)
        {
            Method = methodInfo.CreateDelegate(delegateType, target);
            ParameterCount = methodInfo.GetParameters().Length;
        }

        public BuiltInFunction(Delegate method)
        {
            Method = method;
            ParameterCount = method.Method.GetParameters().Length;
        }

        public BuiltInFunction(Func<Value, Unit> print)
        {
            this.print = print;
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