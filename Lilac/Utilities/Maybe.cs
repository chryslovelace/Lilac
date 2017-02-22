using System;

namespace Lilac.Utilities
{
    public abstract class Maybe<T>
    {
        public static Maybe<T> Nothing { get; } = new Nothing<T>();
        public abstract T GetValueOrDefault();

        public abstract TResult Match<TResult>(Func<T, TResult> justFunc, Func<TResult> nothingFunc);
        public abstract void Match(Action<T> justAction, Action nothingAction);

        public Maybe<TOther> Bind<TOther>(Func<T, Maybe<TOther>> func) => Match(func, () => Maybe<TOther>.Nothing);

        public Maybe<TSelect> SelectMany<TOther, TSelect>(Func<T, Maybe<TOther>> func, Func<T, TOther, TSelect> select)
            => Bind(val1 => func(val1).Bind(val2 => select(val1, val2).ToMaybe()));

        public Maybe<TOther> Select<TOther>(Func<T, TOther> func) => Bind(result => func(result).ToMaybe());

        public Maybe<T> Where(Func<T, bool> func)
            => Bind(val => func(val) ? val.ToMaybe() : Nothing);
    }

    public class Nothing<T> : Maybe<T>
    {
        public override T GetValueOrDefault() => default(T);
        public override TResult Match<TResult>(Func<T, TResult> justFunc, Func<TResult> nothingFunc) => nothingFunc();

        public override void Match(Action<T> justAction, Action nothingAction) => nothingAction();

        public override string ToString() => "Nothing";
    }

    public class Just<T> : Maybe<T>
    {
        public T Value { get; }
        public Just(T value)
        {
            Value = value;
        }

        public override T GetValueOrDefault() => Value;
        public override TResult Match<TResult>(Func<T, TResult> justFunc, Func<TResult> nothingFunc) => justFunc(Value);
        public override void Match(Action<T> justAction, Action nothingAction) => justAction(Value);
        public override string ToString() => Value.ToString();
    }
 
    public static class Maybe
    {
        public static Maybe<T> ToMaybe<T>(this T value) => value != null ? new Just<T>(value) : Maybe<T>.Nothing;
    }
}