using System;

namespace Lilac.Utilities
{
    public abstract class Result<TResult, TError>
    {
        public abstract TOutput Match<TOutput>(Func<TResult, TOutput> okFunc, Func<TError, TOutput> errFunc);
        public abstract void Match(Action<TResult> okAction, Action<TError> errAction);

        public Result<TOther, TError> Bind<TOther>(Func<TResult, Result<TOther, TError>> func)
            => Match(func, err => new Error<TOther, TError>(err));

        public Result<TSelect, TError> SelectMany<TOther, TSelect>(Func<TResult, Result<TOther, TError>> func,
            Func<TResult, TOther, TSelect> select)
            => Bind(val1 => func(val1).Bind(val2 => new Ok<TSelect, TError>(select(val1, val2))));

        public Result<TOther, TError> Select<TOther>(Func<TResult, TOther> func)
            => Bind(val1 => new Ok<TOther, TError>(func(val1)));

        public Result<TResult, TError> Where(Func<TResult, bool> func)
            => Bind(val => func(val)
                ? (Result<TResult, TError>) new Ok<TResult, TError>(val)
                : new Error<TResult, TError>(default(TError)));

        public static Result<TResult, TError> FromMaybe(Maybe<TResult> maybe, TError error)
            => maybe.Match<Result<TResult, TError>>(
                just => new Ok<TResult, TError>(just),
                () => new Error<TResult, TError>(error));
    }
    
    public class Ok<TResult, TError> : Result<TResult, TError>
    {
        public TResult Value { get; }

        public Ok(TResult value)
        {
            Value = value;
        }

        public override TOutput Match<TOutput>(Func<TResult, TOutput> okFunc, Func<TError, TOutput> errFunc) => okFunc(Value);

        public override void Match(Action<TResult> okAction, Action<TError> errAction) => okAction(Value);
    }

    public class Error<TResult, TError> : Result<TResult, TError>
    {
        public TError Value { get; }
        public Error(TError value)
        {
            Value = value;
        }

        public override TOutput Match<TOutput>(Func<TResult, TOutput> okFunc, Func<TError, TOutput> errFunc) => errFunc(Value);

        public override void Match(Action<TResult> okAction, Action<TError> errAction) => errAction(Value);
    }
}