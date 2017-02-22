using System;
using Lilac.Attributes;

namespace Lilac.Values
{
    public class Boolean : Value, IEquatable<Boolean>
    {
        private bool Value { get; }
        
        private Boolean(bool value)
        {
            Value = value;
        }

        [BuiltInValue("true")]
        public static Boolean True { get; } = new Boolean(true);

        [BuiltInValue("false")]
        public static Boolean False { get; } = new Boolean(false);

        public static Boolean Get(bool value) => value ? True : False;

        public override bool AsBool() => Value;

        public override string ToString() => Value.ToString();

        public override bool Equals(object obj)
        {
            return ReferenceEquals(obj, this) || Equals(obj as Boolean);
        }

        public bool Equals(Boolean other)
        {
            if (ReferenceEquals(other, null)) return false;
            return Value == other.Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        [BuiltInFunction("and", typeof(Func<Value, Value, Boolean>), IsOperator = true)]
        public static Boolean And(Value lhs, Value rhs)
        {
            return Get(lhs.AsBool() && rhs.AsBool());
        }

        [BuiltInFunction("or", typeof(Func<Value, Value, Boolean>), IsOperator = true)]
        public static Boolean Or(Value lhs, Value rhs)
        {
            return Get(lhs.AsBool() || rhs.AsBool());
        }

        [BuiltInFunction("not", typeof(Func<Value, Boolean>))]
        public static Boolean Not(Value val)
        {
            return Get(!val.AsBool());
        }

        #region Operators

        public static Boolean operator &(Boolean lhs, Boolean rhs)
        {
            return And(lhs, rhs);
        }

        public static Boolean operator |(Boolean lhs, Boolean rhs)
        {
            return Or(lhs, rhs);
        }

        public static bool operator false(Boolean val)
        {
            return !val.Value;
        }

        public static bool operator true(Boolean val)
        {
            return val.Value;
        }

        public static Boolean operator !(Boolean val)
        {
            return Not(val);
        }

        #endregion

    }
}