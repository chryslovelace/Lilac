using System;
using System.Collections.Immutable;
using System.Linq;
using Lilac.Utilities;

namespace Lilac.Values
{
    public class CurriedFunction : Value
    {
        public Value Callable { get; set; }
        public ImmutableList<Value> AppliedArguments { get; set; }
        public Type ValueType { get; set; }

        public CurriedFunction(Value callable)
        {
            Callable = callable;
            AppliedArguments = ImmutableList<Value>.Empty;
            ValueType = callable.GetValueType();
        }
        
        public CurriedFunction Apply(Value argument)
        {
            return new CurriedFunction(Callable)
            {
                AppliedArguments = AppliedArguments.Add(argument),
                ValueType = ValueType.GetCurriedType()
            };
        }

        public override string ToString()
        {
            return $"<#curried {Callable}/{AppliedArguments.Count}>";
        }

        public override bool IsCallable()
        {
            return Callable.IsCallable();
        }

        public override Type GetValueType()
        {
            return ValueType;
        }
    }
}