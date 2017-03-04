using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Specialized;

namespace Lilac.Tests
{
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