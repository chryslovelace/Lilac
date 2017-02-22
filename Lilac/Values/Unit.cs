using System;
using Lilac.Attributes;

namespace Lilac.Values
{
    public class Unit : Value, IEquatable<Unit>
    {
        private Unit() { }

        [BuiltInValue("nil")]
        public static Unit Value { get; } = new Unit();

        public bool Equals(Unit other) => !ReferenceEquals(other, null);

        public override bool Equals(object obj) => obj is Unit;

        public override int GetHashCode() => 0;

        public override string ToString()
        {
            return "()";
        }
    }
}