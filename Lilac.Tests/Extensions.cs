using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Lilac.Tests
{
    [ExcludeFromCodeCoverage]
    public static class Extensions
    {
        public static Action Enumerating<T>(this IEnumerable<T> source) => () =>
        {
            foreach (var item in source)
            {
            }
        };
    }
}