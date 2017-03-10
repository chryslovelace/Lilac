using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Lilac.Utilities
{
    public static class Extensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default(TValue))
        {
            TValue value;
            return dict.TryGetValue(key, out value) ? value : defaultValue;
        }
        
        public static TResult Map<T1, T2, TResult>(this Tuple<T1, T2> tuple, Func<T1, T2, TResult> func) => func(tuple.Item1, tuple.Item2);
        public static void Map<T1, T2>(this Tuple<T1, T2> tuple, Action<T1, T2> action) => action(tuple.Item1, tuple.Item2);

        public static BigInteger Sum(this IEnumerable<BigInteger> source)
            => source.Aggregate(BigInteger.Zero, (current, bigInteger) => current + bigInteger);

        public static string CamelCaseToKebabCase(this string str)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < str.Length; i++)
            {
                var c = str[i];
                if (Char.IsUpper(c))
                {
                    if (i > 0) sb.Append('-');
                    sb.Append(Char.ToLower(c));
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        public static string PrettyPrintParameters(this IReadOnlyList<string> parameters)
        {
            return parameters.Count == 0 ? "()" : String.Join(" ", parameters);
        }

        public static int[] ToUtf32(this string str)
        {
            var bytes = Encoding.UTF32.GetBytes(str);
            var ints = new int[bytes.Length/4];
            for (var i = 0; i < ints.Length; i++)
            {
                ints[i] = BitConverter.ToInt32(bytes, i*4);
            }
            return ints;
        }

        public static bool In<T>(this T elem, IEnumerable<T> collection) => collection.Contains(elem);

        public static Type GetCurriedType(this Type callableType)
        {
            var typeArgs = callableType.GenericTypeArguments.Skip(1).ToArray();

            return Type.GetType($"System.Func`{typeArgs.Length}", true).MakeGenericType(typeArgs);
        }

        public static string LilacTypeName(this Type type)
        {
            if (typeof(Delegate).IsAssignableFrom(type))
            {
                return type.GenericTypeArguments.Length == 1
                    ? $"unit -> {type.GenericTypeArguments[0].Name.CamelCaseToKebabCase()}"
                    : string.Join(" -> ", type.GenericTypeArguments.Select(a => a.Name.CamelCaseToKebabCase()));
            }
            return type.Name.CamelCaseToKebabCase();
        }
    }
}