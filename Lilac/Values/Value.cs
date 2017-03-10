using System;
using Lilac.Attributes;
using Lilac.Utilities;

namespace Lilac.Values
{
    public abstract class Value : IComparable<Value>
    {
        #region Virtual Methods

        public virtual bool AsBool() => true;

        public virtual bool IsCallable() => false;

        public virtual Value GetMember(string name) => null;

        public virtual bool SetMember(string name, Value value) => false;

        public virtual int CompareTo(Value other)
        {
            throw new Exception($"Type {GetType().Name.CamelCaseToKebabCase()} is not comparable!");
        }

        public virtual Type GetValueType() => GetType();

        #endregion

        #region Built In Functions

        [BuiltInFunction("=", typeof(Func<Value, Value, Boolean>), IsOperator = true)]
        public static Boolean Equals(Value lhs, Value rhs)
        {
            return Boolean.Get(lhs.Equals(rhs));
        }

        [BuiltInFunction("is", typeof(Func<Value, Value, Boolean>), IsOperator = true)]
        public static Boolean Is(Value lhs, Value rhs)
        {
            return Boolean.Get(ReferenceEquals(lhs, rhs));
        }

        [BuiltInFunction("!=", typeof(Func<Value, Value, Boolean>), IsOperator = true)]
        public static Boolean NotEquals(Value lhs, Value rhs)
        {
            return Boolean.Get(!lhs.Equals(rhs));
        }

        [BuiltInFunction("<=", typeof(Func<Value, Value, Boolean>), IsOperator = true)]
        public static Boolean LessThanOrEquals(Value lhs, Value rhs)
        {
            return Boolean.Get(lhs.CompareTo(rhs) <= 0);
        }

        [BuiltInFunction("<", typeof(Func<Value, Value, Boolean>), IsOperator = true)]
        public static Boolean LessThan(Value lhs, Value rhs)
        {
            return Boolean.Get(lhs.CompareTo(rhs) < 0);
        }

        [BuiltInFunction(">", typeof(Func<Value, Value, Boolean>), IsOperator = true)]
        public static Boolean GreaterThan(Value lhs, Value rhs)
        {
            return Boolean.Get(lhs.CompareTo(rhs) > 0);
        }

        [BuiltInFunction(">=", typeof(Func<Value, Value, Boolean>), IsOperator = true)]
        public static Boolean GreaterThanOrEquals(Value lhs, Value rhs)
        {
            return Boolean.Get(lhs.CompareTo(rhs) >= 0);
        }

        [BuiltInFunction("number?", typeof(Func<Value, Boolean>))]
        public static Boolean IsNumber(Value value)
        {
            return Boolean.Get(value is Number);
        }

        [BuiltInFunction("boolean?", typeof(Func<Value, Boolean>))]
        public static Boolean IsBoolean(Value value)
        {
            return Boolean.Get(value is Boolean);
        }

        [BuiltInFunction("string?", typeof(Func<Value, Boolean>))]
        public static Boolean IsString(Value value)
        {
            return Boolean.Get(value is String);
        }

        [BuiltInFunction("char?", typeof(Func<Value, Boolean>))]
        public static Boolean IsChar(Value value)
        {
            return Boolean.Get(value is Char);
        }

        [BuiltInFunction("list?", typeof(Func<Value, Boolean>))]
        public static Boolean IsList(Value value)
        {
            return Boolean.Get(value is List);
        }

        [BuiltInFunction("pair?", typeof(Func<Value, Boolean>))]
        public static Boolean IsPair(Value value)
        {
            return Boolean.Get(value is Pair);
        }

        [BuiltInFunction("linked-list?", typeof(Func<Value, Boolean>))]
        public static Boolean IsLinkedList(Value value)
        {
            var pair = value as Pair;
            return Boolean.Get(value is Unit || (pair != null && IsLinkedList(pair.CdrValue).AsBool()));
        }

        [BuiltInFunction("unit?", typeof(Func<Value, Boolean>))]
        public static Boolean IsUnit(Value value)
        {
            return Boolean.Get(value is Unit);
        }

        [BuiltInFunction("callable?", typeof(Func<Value, Boolean>))]
        public static Boolean IsCallable(Value value)
        {
            return Boolean.Get(value.IsCallable());
        }

        [BuiltInFunction("complex?", typeof(Func<Value, Boolean>))]
        public static Boolean IsComplexNumber(Value value)
        {
            var number = value as Number;
            return Boolean.Get(number?.IsComplex == true);
        }

        [BuiltInFunction("real?", typeof(Func<Value, Boolean>))]
        public static Boolean IsRealNumber(Value value)
        {
            var number = value as Number;
            return Boolean.Get(number?.IsReal == true);
        }

        [BuiltInFunction("rational?", typeof(Func<Value, Boolean>))]
        public static Boolean IsRationalNumber(Value value)
        {
            var number = value as Number;
            return Boolean.Get(number?.IsRational == true);
        }

        [BuiltInFunction("integer?", typeof(Func<Value, Boolean>))]
        public static Boolean IsIntegerNumber(Value value)
        {
            var number = value as Number;
            return Boolean.Get(number?.IsInteger == true);
        }

        [BuiltInFunction("native-int?", typeof(Func<Value, Boolean>))]
        public static Boolean IsNativeIntNumber(Value value)
        {
            var number = value as Number;
            return Boolean.Get(number?.IsNativeInt == true);
        }

        [BuiltInFunction("print", typeof(Func<Value, Unit>))]
        public static Unit Print(Value value)
        {
            Console.Write(value);
            return Unit.Value;
        }

        [BuiltInFunction("println", typeof(Func<Value, Unit>))]
        public static Unit PrintLn(Value value)
        {
            Console.WriteLine(value);
            return Unit.Value;
        }

        [BuiltInFunction("typeof", typeof(Func<Value, String>))]
        public static String TypeOf(Value value)
        {
            var num = value as Number;
            var typename = num?.Type.ToString() ?? value.GetType().Name;

            return String.Get(typename.CamelCaseToKebabCase());
        }

        #endregion
    }
}