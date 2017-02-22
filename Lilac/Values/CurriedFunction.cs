using System.Collections.Immutable;

namespace Lilac.Values
{
    public class CurriedFunction : Value
    {
        public Value Callable { get; set; }
        public ImmutableList<Value> AppliedArguments { get; set; }

        public CurriedFunction(Value callable)
        {
            Callable = callable;
            AppliedArguments = ImmutableList<Value>.Empty;
        }
        
        public CurriedFunction Apply(Value argument)
        {
            return new CurriedFunction(Callable)
            {
                AppliedArguments = AppliedArguments.Add(argument)
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
    }
}