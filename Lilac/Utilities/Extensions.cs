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
                if (char.IsUpper(c))
                {
                    if (i > 0) sb.Append('-');
                    sb.Append(char.ToLower(c));
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
            return parameters.Count == 0 ? "()" : string.Join(" ", parameters);
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
    }
}