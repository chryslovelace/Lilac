using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Lilac.Attributes;
using Lilac.Exceptions;
using Lilac.Utilities;
using Numerics;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace Lilac.Values
{
    public enum NumberType
    {
        NativeInt,
        Integer,
        Rational,
        Real,
        Complex
    }

    public class Number : Value, IComparable<Number>, IEquatable<Number>, IComparable
    {
        #region Constructor

        private Number(NumberType type, object value)
        {
            Type = type;
            Value = value;
        }

        #endregion

        #region Private Properties

        private object Value { get; }

        #endregion

        #region Private Instance Methods

        private Number Raise()
        {
            switch (Type)
            {
                case NumberType.NativeInt:
                    return Integer((long)Value);
                case NumberType.Integer:
                    return Rational(new BigRational((BigInteger)Value));
                case NumberType.Rational:
                    return Real((double)(BigRational)Value);
                case NumberType.Real:
                    return Complex((double)Value);
                case NumberType.Complex:
                    throw new NumericTypeException("Cannot raise numeric value of type complex!");
                default:
                    throw new NumericTypeException($"Invalid numeric type {Type}!");
            }
        }

        private Number Lower()
        {
            switch (Type)
            {
                case NumberType.NativeInt:
                    throw new NumericTypeException("Cannot lower numeric value of type native-int!");
                case NumberType.Integer:
                    return NativeInt((long)(BigInteger)Value);
                case NumberType.Rational:
                    return Integer(((BigRational)Value).GetWholePart());
                case NumberType.Real:
                    return Rational(RealToRational((double)Value));
                case NumberType.Complex:
                    return Real(((Complex)Value).Real);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Number LowerIfExact()
        {
            switch (Type)
            {
                case NumberType.NativeInt:
                    throw new NumericTypeException("Cannot lower numeric value of type native-int!");
                case NumberType.Integer:
                    return LowerIfExactInteger();
                case NumberType.Rational:
                    return LowerIfExactRational();
                case NumberType.Real:
                    return LowerIfExactReal();
                case NumberType.Complex:
                    return LowerIfExactComplex();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Number LowerIfExactComplex()
        {
            var value = (Complex)Value;
            return value.Imaginary == 0 ? Real(value.Real) : this;
        }

        private Number LowerIfExactReal()
        {
            var value = (double)Value;
            return double.IsInfinity(value) || double.IsNaN(value) ? this : Rational(RealToRational(value));
        }

        private Number LowerIfExactRational()
        {
            var value = (BigRational)Value;
            return IsIntegral(value) ? Integer(value.GetWholePart()) : this;
        }

        private Number LowerIfExactInteger()
        {
            try
            {
                return NativeInt((long)(BigInteger)Value);
            }
            catch (OverflowException)
            {
                return this;
            }
        }

        private Number RaiseTo(NumberType type)
        {
            var num = this;
            while (num.Type < type)
                num = num.Raise();
            return num;
        }

        private Number LowerTo(NumberType type)
        {
            var num = this;
            while (num.Type > type)
                num = num.Lower();
            return num;
        }

        private Number CoerceTo(NumberType type)
        {
            return RaiseTo(type).LowerTo(type);
        }

        #endregion

        #region Private Static Methods

        private static bool IsIntegral(BigRational val)
        {
            return val.GetFractionPart() == BigRational.Zero;
        }

        private static BigRational RealToRational(double value)
        {
            if (double.IsInfinity(value) || double.IsNaN(value))
                throw new NotFiniteNumberException("Cannot convert non-finite number to exact representation!", value);
            var str = value.ToString("R");
            var radixIndex = str.IndexOf(".", StringComparison.Ordinal);
            var eIndex = str.IndexOf("e", StringComparison.OrdinalIgnoreCase);
            if (eIndex == -1)
            {
                var numer = BigInteger.Parse(str.Replace(".", ""));
                if (radixIndex == -1)
                    return new BigRational(numer);
                var denom = BigInteger.Pow(10, str.Length - radixIndex - 1);
                return new BigRational(numer, denom);
            }
            else
            {
                var exponent = int.Parse(str.Substring(eIndex + 1));
                var numer = BigInteger.Parse(str.Substring(0, eIndex).Replace(".", ""));
                var denom = BigInteger.Pow(10, eIndex - radixIndex - 1);
                var power = BigInteger.Pow(10, Math.Abs(exponent));
                if (exponent > 0)
                    numer *= power;
                else
                    denom *= power;
                return new BigRational(numer, denom);
            }
        }

        private static void RaiseToSame([NotNull] ref Number lhs, [NotNull] ref Number rhs)
        {
            if (lhs == null)
                throw new ArgumentNullException(nameof(lhs));
            if (rhs == null)
                throw new ArgumentNullException(nameof(rhs));
            if (lhs.Type > rhs.Type)
            {
                rhs = rhs.RaiseTo(lhs.Type);
            }
            else if (rhs.Type > lhs.Type)
            {
                lhs = lhs.RaiseTo(rhs.Type);
            }
        }

        private static Number AddNativeInts(Number lhs, Number rhs)
        {
            try
            {
                return NativeInt(checked((long)lhs.Value + (long)rhs.Value));
            }
            catch (OverflowException)
            {
                return Integer(BigInteger.Add((long)lhs.Value, (long)rhs.Value));
            }
        }

        private static Number SubtractNativeInts(Number lhs, Number rhs)
        {
            try
            {
                return NativeInt(checked((long)lhs.Value - (long)rhs.Value));
            }
            catch (OverflowException)
            {
                return Integer(BigInteger.Subtract((long)lhs.Value, (long)rhs.Value));
            }
        }

        private static Number MultiplyNativeInts(Number lhs, Number rhs)
        {
            try
            {
                return NativeInt(checked((long)lhs.Value * (long)rhs.Value));
            }
            catch (OverflowException)
            {
                return Integer(BigInteger.Multiply((long)lhs.Value, (long)rhs.Value));
            }
        }

        private static Number DivideNativeInts(Number lhs, Number rhs)
        {
            long remainder;
            var numer = (long)lhs.Value;
            var denom = (long)rhs.Value;
            var val = Math.DivRem(numer, denom, out remainder);
            return remainder == 0 ? NativeInt(val) : Rational(numer, denom);
        }

        private static Number DivideIntegers(Number lhs, Number rhs)
        {
            var numer = (BigInteger)lhs.Value;
            var denom = (BigInteger)rhs.Value;
            var rational = new BigRational(numer, denom);
            return IsIntegral(rational) ? Integer(rational.GetWholePart()) : Rational(rational);
        }

        private static Number PowNativeInts(Number lhs, Number rhs)
        {
            var lval = (long)lhs.Value;
            var rval = (long)rhs.Value;
            try
            {
                return Integer(BigInteger.Pow(lval, (int)rval)).LowerIfExactInteger();
            }
            catch (OverflowException)
            {
                return Real(Math.Pow(lval, rval));
            }
        }

        private static Number PowIntegers(Number lhs, Number rhs)
        {
            var lval = (BigInteger)lhs.Value;
            var rval = (BigInteger)rhs.Value;
            try
            {
                return Integer(BigInteger.Pow(lval, (int)rval));
            }
            catch (OverflowException)
            {
                return Real(Math.Pow((double)lval, (double)rval));
            }
        }

        private static Number PowRationals(Number lhs, Number rhs)
        {
            var lval = (BigRational)lhs.Value;
            var rval = (BigRational)rhs.Value;
            return IsIntegral(rval) ? Rational(BigRational.Pow(lval, rval.GetWholePart())) : Real(Math.Pow((double)lval, (double)rval));
        }

        private static Number RoundRational(Number number)
        {
            var value = (BigRational)number.Value;
            var integer = value.GetWholePart();
            var fraction = value.GetFractionPart();
            if (fraction == new BigRational(1, 2) && !integer.IsEven)
                ++integer;
            if (fraction == new BigRational(-1, 2) && !integer.IsEven)
                --integer;
            return Integer(integer);
        }

        #endregion

        #region Public Factory Methods

        public static Number NativeInt(byte value) => new Number(NumberType.NativeInt, (long) value);

        public static Number NativeInt(short value) => new Number(NumberType.NativeInt, (long) value);

        public static Number NativeInt(int value) => new Number(NumberType.NativeInt, (long) value);

        public static Number NativeInt(long value) => new Number(NumberType.NativeInt, value);

        public static Number Integer(BigInteger value) => new Number(NumberType.Integer, value);

        public static Number Rational(BigInteger numer, BigInteger denom) => new Number(NumberType.Rational, new BigRational(numer, denom));

        public static Number Rational(BigRational value) => new Number(NumberType.Rational, value);

        public static Number Real(double value) => new Number(NumberType.Real, value);

        public static Number Complex(double value) => new Number(NumberType.Complex, new Complex(value, 0));

        public static Number Complex(Complex value) => new Number(NumberType.Complex, value);

        #endregion

        #region Public Properties

        public NumberType Type { get; }

        public bool IsComplex => Type <= NumberType.Complex;

        public bool IsReal => ImagPart(this) == Zero;

        public bool IsRational => IsReal && IsFinite(this).AsBool();

        public bool IsInteger
        {
            get
            {
                switch (Type)
                {
                    case NumberType.NativeInt:
                    case NumberType.Integer:
                        return true;
                    case NumberType.Rational:
                        return IsIntegral((BigRational)Value);
                    case NumberType.Real:
                    case NumberType.Complex:
                        return IsRational && RealPart(this) == Truncate(RealPart(this));
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
            }
        }

        public bool IsNativeInt => Type == NumberType.NativeInt;

        #endregion

        #region Public Static Properties

        [BuiltInValue("zero", Namespace = "math")]
        public static Number Zero { get; } = NativeInt(0);

        [BuiltInValue("inf", Namespace = "math"), BuiltInValue("+inf", Namespace = "math")]
        public static Number PositiveInfinity { get; } = Real(double.PositiveInfinity);

        [BuiltInValue("-inf", Namespace = "math")]
        public static Number NegativeInfinity { get; } = Real(double.NegativeInfinity);

        [BuiltInValue("nan", Namespace = "math")]
        public static Number NotANumber { get; } = Real(double.NaN);

        [BuiltInValue("epsilon", Namespace = "math")]
        public static Number Epsilon { get; } = Real(double.Epsilon);

        [BuiltInValue("pi", Namespace = "math")]
        public static Number Pi { get; } = Real(Math.PI);

        [BuiltInValue("e", Namespace = "math")]
        public static Number E { get; } = Real(Math.E);

        [BuiltInValue("i", Namespace = "math")]
        public static Number I { get; } = Complex(new Complex(0, 1));

        [BuiltInValue("-i", Namespace = "math")]
        public static Number MinusI { get; } = Complex(new Complex(0, -1));

        #endregion

        #region Public Instance Methods

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType() == GetType() && Equals((Number) obj);
        }

        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as Number);
        }

        public int CompareTo(Number other)
        {
            if (ReferenceEquals(other, null))
                return 1;
            var lhs = this;
            RaiseToSame(ref lhs, ref other);
            switch (lhs.Type)
            {
                case NumberType.NativeInt:
                    return ((long) lhs.Value).CompareTo((long) other.Value);
                case NumberType.Integer:
                    return ((BigInteger) lhs.Value).CompareTo((BigInteger) other.Value);
                case NumberType.Rational:
                    return ((BigRational) lhs.Value).CompareTo((BigRational) other.Value);
                case NumberType.Real:
                    return ((double) lhs.Value).CompareTo((double) other.Value);
                case NumberType.Complex:
                    throw new NumericTypeException("Ordering is not defined for complex numbers!");
                default:
                    throw new NumericTypeException($"Invalid numeric type {lhs.Type}!");
            }
        }

        public override int CompareTo(Value other)
        {
            if (!(other is Number))
                throw new Exception($"Cannot compare number to {other.GetType().Name.CamelCaseToKebabCase()}!");
            return CompareTo((Number) other);
        }

        public bool Equals(Number other)
        {
            if (ReferenceEquals(other, null))
                return false;
            if (ReferenceEquals(other, this))
                return true;
            var lhs = this;
            RaiseToSame(ref lhs, ref other);
            switch (lhs.Type)
            {
                case NumberType.NativeInt:
                    return (long) lhs.Value == (long) other.Value;
                case NumberType.Integer:
                    return (BigInteger) lhs.Value == (BigInteger) other.Value;
                case NumberType.Rational:
                    return (BigRational) lhs.Value == (BigRational) other.Value;
                case NumberType.Real:
                    return (double) lhs.Value == (double) other.Value;
                case NumberType.Complex:
                    return (Complex) lhs.Value == (Complex) other.Value;
                default:
                    throw new NumericTypeException($"Invalid numeric type {lhs.Type}!");
            }
        }

        public override string ToString()
        {
            switch (Type)
            {
                case NumberType.NativeInt:
                case NumberType.Integer:
                case NumberType.Rational:
                    return Value.ToString();
                case NumberType.Real:
                    return ((double) Value).ToString("R");
                case NumberType.Complex:
                    var complex = (Complex) Value;
                    var plus = complex.Imaginary < 0 ? "" : "+";
                    return $"{complex.Real:R}{plus}{complex.Imaginary:R}i";
                default:
                    throw new NumericTypeException($"Invalid numeric type {Type}!");
            }
        }

        public int AsInt32()
        {
            if (!IsInteger)
                throw new NumericTypeException($"Cannot convert {Type} to Int32!");
            try
            {
            var num = CoerceTo(NumberType.NativeInt);
                return checked ((int) (long) num.Value);
            }
            catch (OverflowException e)
            {
                throw new Exception("Value is too large to fit in an Int32!", e);
            }
        }

        #endregion

        #region Operators

        public static Number operator +(Number lhs, Number rhs) => Add(lhs, rhs);

        public static Number operator -(Number lhs, Number rhs) => Subtract(lhs, rhs);

        public static Number operator -(Number val) => Negate(val);

        public static Number operator *(Number lhs, Number rhs) => Multiply(lhs, rhs);

        public static Number operator /(Number lhs, Number rhs) => Divide(lhs, rhs);

        public static Number operator %(Number lhs, Number rhs) => Modulo(lhs, rhs);

        public static bool operator ==(Number lhs, Number rhs) => ReferenceEquals(lhs, null) ? ReferenceEquals(rhs, null) : lhs.Equals(rhs);

        public static bool operator !=(Number lhs, Number rhs) => !(lhs == rhs);

        public static bool operator >=(Number lhs, Number rhs) => lhs?.CompareTo(rhs) >= 0;

        public static bool operator <=(Number lhs, Number rhs) => lhs?.CompareTo(rhs) <= 0;

        public static bool operator >(Number lhs, Number rhs) => lhs?.CompareTo(rhs) > 0;
        public static bool operator <(Number lhs, Number rhs) => lhs?.CompareTo(rhs) < 0;

        #endregion

        #region Public Static Methods

        public static Number Parse(string value)
        {
            if (value.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
                return ParseHexLiteral(value);
            if (value.StartsWith("0b", StringComparison.InvariantCultureIgnoreCase))
                return ParseBinaryLiteral(value);
            if (value.EndsWith("i"))
                return ParseComplexLiteral(value);
            if (value.Contains("/"))
                return ParseRationalLiteral(value);
            return ParseDecimalLiteral(value);
        }

        [BuiltInFunction("string->number", typeof(Func<String, Number>))]
        public static Number Parse(String str)
        {
            return Parse(str.ToString());
        }

        public static Number ParseRationalLiteral(string value)
        {
            var match = Regex.Match(value, @"^\s*(-?\d*)\s*/\s*(\d*)\s*$");
            if (!match.Success)
                throw new ParseException($"Failed to parse rational number from '{value}'!");
            var numer = BigInteger.Parse(match.Groups[1].Value);
            var denom = BigInteger.Parse(match.Groups[2].Value);
            if (numer == 0 && denom == 0)
                return NotANumber;
            if (numer == 0)
                return Zero;
            if (denom == 0)
                return Real(numer.Sign*double.PositiveInfinity);
            var rational = new BigRational(numer, denom);
            return IsIntegral(rational) ? Integer(rational.GetWholePart()) : Rational(rational);
        }

        public static Number ParseComplexLiteral(string value)
        {
            var match = Regex.Match(value, @"^\s*(\S*)\s*([+-])\s*(\S*)i\s*$");
            if (!match.Success)
                throw new ParseException($"Failed to parse complex number from '{value}'!");
            var realPart = double.Parse(match.Groups[1].Value);
            var imagSign = match.Groups[2].Value == "+" ? 1 : -1;
            var imagPart = double.Parse(match.Groups[3].Value);
            return Complex(new Complex(realPart, imagSign*imagPart));
        }

        public static Number ParseDecimalLiteral(string value)
        {
            if (value.Contains(".") || value.Contains("e") || value.Contains("E"))
            {
                return Real(double.Parse(value));
            }
            try
            {
                return NativeInt(long.Parse(value));
            }
            catch (OverflowException)
            {
                return Integer(BigInteger.Parse(value));
            }
        }

        public static Number ParseBinaryLiteral(string value)
        {
            var bits = value.Skip(2).Reverse().Select(c => c == '0' ? 0 : 1).ToList();
            try
            {
                return NativeInt(bits.Select((b, i) => (long) b << i).Sum());
            }
            catch (OverflowException)
            {
                return Integer(bits.Select((b, i) => b*BigInteger.Pow(2, i)).Sum());
            }
        }

        public static Number ParseHexLiteral(string value)
        {
            value = value.Substring(2);
            try
            {
                return NativeInt(long.Parse(value, NumberStyles.HexNumber));
            }
            catch (OverflowException)
            {
                return Integer(BigInteger.Parse(value, NumberStyles.HexNumber));
            }
        }

        [BuiltInFunction("exact", typeof(Func<Number, Number>), Namespace = "math")]
        public static Number Exact(Number num)
        {
            num = num.LowerTo(NumberType.Rational);
            if (num.Type == NumberType.Rational && IsIntegral((BigRational) num.Value))
                num = num.Lower();
            return num;
        }

        [BuiltInFunction("inexact", typeof(Func<Number, Number>), Namespace = "math")]
        public static Number InExact(Number num) => num.RaiseTo(NumberType.Real);

        [BuiltInFunction("+", typeof(Func<Number, Number, Number>), IsOperator = true)]
        public static Number Add(Number lhs, Number rhs)
        {
            RaiseToSame(ref lhs, ref rhs);
            switch (lhs.Type)
            {
                case NumberType.NativeInt:
                    return AddNativeInts(lhs, rhs);
                case NumberType.Integer:
                    return Integer(BigInteger.Add((BigInteger) lhs.Value, (BigInteger) rhs.Value));
                case NumberType.Rational:
                    return Rational((BigRational) lhs.Value + (BigRational) rhs.Value);
                case NumberType.Real:
                    return Real((double) lhs.Value + (double) rhs.Value);
                case NumberType.Complex:
                    return Complex((Complex) lhs.Value + (Complex) rhs.Value);
                default:
                    throw new NumericTypeException($"Invalid numeric type {lhs.Type}!");
            }
        }

        [BuiltInFunction("-", typeof(Func<Number, Number, Number>), IsOperator = true)]
        public static Number Subtract(Number lhs, Number rhs)
        {
            RaiseToSame(ref lhs, ref rhs);
            switch (lhs.Type)
            {
                case NumberType.NativeInt:
                    return SubtractNativeInts(lhs, rhs);
                case NumberType.Integer:
                    return Integer(BigInteger.Subtract((BigInteger) lhs.Value, (BigInteger) rhs.Value));
                case NumberType.Rational:
                    return Rational((BigRational) lhs.Value - (BigRational) rhs.Value);
                case NumberType.Real:
                    return Real((double) lhs.Value - (double) rhs.Value);
                case NumberType.Complex:
                    return Complex((Complex) lhs.Value - (Complex) rhs.Value);
                default:
                    throw new NumericTypeException($"Invalid numeric type {lhs.Type}!");
            }
        }

        [BuiltInFunction("negate", typeof(Func<Number, Number>), Namespace = "math")]
        public static Number Negate(Number val)
        {
            switch (val.Type)
            {
                case NumberType.NativeInt:
                    return NativeInt(-(long) val.Value);
                case NumberType.Integer:
                    return Integer(-(BigInteger) val.Value);
                case NumberType.Rational:
                    return Rational(-(BigRational) val.Value);
                case NumberType.Real:
                    return Real(-(double) val.Value);
                case NumberType.Complex:
                    return Complex(-(Complex) val.Value);
                default:
                    throw new NumericTypeException($"Invalid numeric type {val.Type}!");
            }
        }

        [BuiltInFunction("*", typeof(Func<Number, Number, Number>), IsOperator = true)]
        public static Number Multiply(Number lhs, Number rhs)
        {
            RaiseToSame(ref lhs, ref rhs);
            switch (lhs.Type)
            {
                case NumberType.NativeInt:
                    return MultiplyNativeInts(lhs, rhs);
                case NumberType.Integer:
                    return Integer(BigInteger.Multiply((BigInteger) lhs.Value, (BigInteger) rhs.Value));
                case NumberType.Rational:
                    return Rational((BigRational) lhs.Value*(BigRational) rhs.Value);
                case NumberType.Real:
                    return Real((double) lhs.Value*(double) rhs.Value);
                case NumberType.Complex:
                    return Complex((Complex) lhs.Value*(Complex) rhs.Value);
                default:
                    throw new NumericTypeException($"Invalid numeric type {lhs.Type}!");
            }
        }

        [BuiltInFunction("/", typeof(Func<Number, Number, Number>), IsOperator = true)]
        public static Number Divide(Number lhs, Number rhs)
        {
            if (rhs == Zero)
                return DivideByZero(lhs);
            RaiseToSame(ref lhs, ref rhs);
            switch (lhs.Type)
            {
                case NumberType.NativeInt:
                    return DivideNativeInts(lhs, rhs);
                case NumberType.Integer:
                    return DivideIntegers(lhs, rhs);
                case NumberType.Rational:
                    return Rational((BigRational) lhs.Value/(BigRational) rhs.Value);
                case NumberType.Real:
                    return Real((double) lhs.Value/(double) rhs.Value);
                case NumberType.Complex:
                    return Complex((Complex) lhs.Value/(Complex) rhs.Value);
                default:
                    throw new NumericTypeException($"Invalid numeric type {lhs.Type}!");
            }
        }

        private static Number DivideByZero(Number lhs)
        {
            switch (lhs.Type)
            {
                case NumberType.NativeInt:
                case NumberType.Integer:
                case NumberType.Rational:
                case NumberType.Real:
                    if (lhs == Zero)
                        return NotANumber;
                    return lhs > Zero ? PositiveInfinity : NegativeInfinity;
                case NumberType.Complex:
                    return Complex((Complex) lhs.Value/0);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [BuiltInFunction("%", typeof(Func<Number, Number, Number>), IsOperator = true)]
        public static Number Modulo(Number lhs, Number rhs)
        {
            RaiseToSame(ref lhs, ref rhs);
            switch (lhs.Type)
            {
                case NumberType.NativeInt:
                    return NativeInt((long) lhs.Value%(long) rhs.Value);
                case NumberType.Integer:
                    return Integer(BigInteger.Remainder((BigInteger) lhs.Value, (BigInteger) rhs.Value));
                case NumberType.Rational:
                    return Rational((BigRational) lhs.Value%(BigRational) rhs.Value);
                case NumberType.Real:
                    return Real(Math.IEEERemainder((double) lhs.Value, (double) rhs.Value));
                case NumberType.Complex:
                    throw new NumericTypeException("Modulo operation is not defined for complex numbers!");
                default:
                    throw new NumericTypeException($"Invalid numeric type {lhs.Type}!");
            }
        }

        [BuiltInFunction("^", typeof(Func<Number, Number, Number>), IsOperator = true)]
        public static Number Pow(Number lhs, Number rhs)
        {
            RaiseToSame(ref lhs, ref rhs);
            switch (lhs.Type)
            {
                case NumberType.NativeInt:
                    return PowNativeInts(lhs, rhs);
                case NumberType.Integer:
                    return PowIntegers(lhs, rhs);
                case NumberType.Rational:
                    return PowRationals(lhs, rhs);
                case NumberType.Real:
                    return Real(Math.Pow((double) lhs.Value, (double) rhs.Value));
                case NumberType.Complex:
                    return Complex(System.Numerics.Complex.Pow((Complex) lhs.Value, (Complex) rhs.Value));
                default:
                    throw new NumericTypeException($"Invalid numeric type {lhs.Type}!");
            }
        }

        [BuiltInFunction("exact?", typeof(Func<Number, Boolean>), Namespace = "math")]
        public static Boolean IsExact(Number number)
        {
            return Boolean.Get(number.Type < NumberType.Real);
        }

        [BuiltInFunction("inexact?", typeof(Func<Number, Boolean>), Namespace = "math")]
        public static Boolean IsInexact(Number number)
        {
            return Boolean.Get(number.Type >= NumberType.Real);
        }

        [BuiltInFunction("zero?", typeof(Func<Number, Boolean>), Namespace = "math")]
        public static Boolean IsZero(Number number)
        {
            return Boolean.Get(number == Zero);
        }

        [BuiltInFunction("positive?", typeof(Func<Number, Boolean>), Namespace = "math")]
        public static Boolean IsPositive(Number number)
        {
            return Boolean.Get(number > Zero);
        }

        [BuiltInFunction("negative?", typeof(Func<Number, Boolean>), Namespace = "math")]
        public static Boolean IsNegative(Number number)
        {
            return Boolean.Get(number < Zero);
        }

        [BuiltInFunction("even?", typeof(Func<Number, Boolean>), Namespace = "math")]
        public static Boolean IsEven(Number number)
        {
            switch (number.Type)
            {
                case NumberType.NativeInt:
                case NumberType.Integer:
                case NumberType.Rational:
                case NumberType.Real:
                    return Boolean.Get(number%NativeInt(2) == Zero);
                case NumberType.Complex:
                    return Boolean.False;
                default:
                    throw new NumericTypeException($"Invalid numeric type {number.Type}!");
            }
        }

        [BuiltInFunction("odd?", typeof(Func<Number, Boolean>), Namespace = "math")]
        public static Boolean IsOdd(Number number)
        {
            switch (number.Type)
            {
                case NumberType.NativeInt:
                case NumberType.Integer:
                case NumberType.Rational:
                case NumberType.Real:
                    return Boolean.Get(number%NativeInt(2) == NativeInt(1));
                case NumberType.Complex:
                    return Boolean.False;
                default:
                    throw new NumericTypeException($"Invalid numeric type {number.Type}!");
            }
        }

        [BuiltInFunction("infinite?", typeof(Func<Number, Boolean>), Namespace = "math")]
        public static Boolean IsInfinite(Number number)
        {
            switch (number.Type)
            {
                case NumberType.NativeInt:
                case NumberType.Integer:
                case NumberType.Rational:
                    return Boolean.False;
                case NumberType.Real:
                    var val = (double) number.Value;
                    return Boolean.Get(double.IsInfinity(val));
                case NumberType.Complex:
                    var cmp = (Complex) number.Value;
                    return Boolean.Get(double.IsInfinity(cmp.Real) || double.IsInfinity(cmp.Imaginary));
                default:
                    throw new NumericTypeException($"Invalid numeric type {number.Type}!");
            }
        }

        [BuiltInFunction("nan?", typeof(Func<Number, Boolean>), Namespace = "math")]
        public static Boolean IsNan(Number number)
        {
            switch (number.Type)
            {
                case NumberType.NativeInt:
                case NumberType.Integer:
                case NumberType.Rational:
                    return Boolean.False;
                case NumberType.Real:
                    var val = (double) number.Value;
                    return Boolean.Get(double.IsNaN(val));
                case NumberType.Complex:
                    var cmp = (Complex) number.Value;
                    return Boolean.Get(double.IsNaN(cmp.Real) || double.IsNaN(cmp.Imaginary));
                default:
                    throw new NumericTypeException($"Invalid numeric type {number.Type}!");
            }
        }

        [BuiltInFunction("finite?", typeof(Func<Number, Boolean>), Namespace = "math")]
        public static Boolean IsFinite(Number number)
        {
            return !(IsInfinite(number) || IsNan(number));
        }

        [BuiltInFunction("max", typeof(Func<Number, Number, Number>), Namespace = "math")]
        public static Number Max(Number lhs, Number rhs)
        {
            return lhs >= rhs ? lhs : rhs;
        }

        [BuiltInFunction("min", typeof(Func<Number, Number, Number>), Namespace = "math")]
        public static Number Min(Number lhs, Number rhs)
        {
            return lhs <= rhs ? lhs : rhs;
        }

        [BuiltInFunction("abs", typeof(Func<Number, Number>), Namespace = "math")]
        public static Number Abs(Number number)
        {
            switch (number.Type)
            {
                case NumberType.NativeInt:
                    return NativeInt(Math.Abs((long) number.Value));
                case NumberType.Integer:
                    return Integer(BigInteger.Abs((BigInteger) number.Value));
                case NumberType.Rational:
                    return Rational(BigRational.Abs((BigRational) number.Value));
                case NumberType.Real:
                    return Real(Math.Abs((double) number.Value));
                case NumberType.Complex:
                    return Real(((Complex) number.Value).Magnitude);
                default:
                    throw new NumericTypeException($"Invalid numeric type {number.Type}!");
            }
        }

        [BuiltInFunction("exp", typeof(Func<Number, Number>), Namespace = "math")]
        public static Number Exp(Number number)
        {
            number = number.RaiseTo(NumberType.Real);
            switch (number.Type)
            {
                case NumberType.Real:
                    return Real(Math.Exp((double) number.Value));
                case NumberType.Complex:
                    return Complex(System.Numerics.Complex.Exp((Complex) number.Value));
                default:
                    throw new NumericTypeException($"Invalid numeric type {number.Type}!");
            }
        }

        [BuiltInFunction("sin", typeof(Func<Number, Number>), Namespace = "math")]
        public static Number Sin(Number number)
        {
            number = number.RaiseTo(NumberType.Real);
            switch (number.Type)
            {
                case NumberType.Real:
                    return Real(Math.Sin((double) number.Value));
                case NumberType.Complex:
                    return Complex(System.Numerics.Complex.Sin((Complex) number.Value));
                default:
                    throw new NumericTypeException($"Invalid numeric type {number.Type}!");
            }
        }

        [BuiltInFunction("cos", typeof(Func<Number, Number>), Namespace = "math")]
        public static Number Cos(Number number)
        {
            number = number.RaiseTo(NumberType.Real);
            switch (number.Type)
            {
                case NumberType.Real:
                    return Real(Math.Cos((double) number.Value));
                case NumberType.Complex:
                    return Complex(System.Numerics.Complex.Cos((Complex) number.Value));
                default:
                    throw new NumericTypeException($"Invalid numeric type {number.Type}!");
            }
        }

        [BuiltInFunction("tan", typeof(Func<Number, Number>), Namespace = "math")]
        public static Number Tan(Number number)
        {
            number = number.RaiseTo(NumberType.Real);
            switch (number.Type)
            {
                case NumberType.Real:
                    return Real(Math.Tan((double) number.Value));
                case NumberType.Complex:
                    return Complex(System.Numerics.Complex.Tan((Complex) number.Value));
                default:
                    throw new NumericTypeException($"Invalid numeric type {number.Type}!");
            }
        }

        [BuiltInFunction("asin", typeof(Func<Number, Number>), Namespace = "math")]
        public static Number Asin(Number number)
        {
            number = number.RaiseTo(NumberType.Real);
            switch (number.Type)
            {
                case NumberType.Real:
                    return Real(Math.Asin((double) number.Value));
                case NumberType.Complex:
                    return Complex(System.Numerics.Complex.Asin((Complex) number.Value));
                default:
                    throw new NumericTypeException($"Invalid numeric type {number.Type}!");
            }
        }

        [BuiltInFunction("acos", typeof(Func<Number, Number>), Namespace = "math")]
        public static Number Acos(Number number)
        {
            number = number.RaiseTo(NumberType.Real);
            switch (number.Type)
            {
                case NumberType.Real:
                    return Real(Math.Acos((double) number.Value));
                case NumberType.Complex:
                    return Complex(System.Numerics.Complex.Acos((Complex) number.Value));
                default:
                    throw new NumericTypeException($"Invalid numeric type {number.Type}!");
            }
        }

        [BuiltInFunction("atan", typeof(Func<Number, Number>), Namespace = "math")]
        public static Number Atan(Number number)
        {
            number = number.RaiseTo(NumberType.Real);
            switch (number.Type)
            {
                case NumberType.Real:
                    return Real(Math.Atan((double) number.Value));
                case NumberType.Complex:
                    return Complex(System.Numerics.Complex.Atan((Complex) number.Value));
                default:
                    throw new NumericTypeException($"Invalid numeric type {number.Type}!");
            }
        }

        [BuiltInFunction("sinh", typeof(Func<Number, Number>), Namespace = "math")]
        public static Number Sinh(Number number)
        {
            number = number.RaiseTo(NumberType.Real);
            switch (number.Type)
            {
                case NumberType.Real:
                    return Real(Math.Sinh((double) number.Value));
                case NumberType.Complex:
                    return Complex(System.Numerics.Complex.Sinh((Complex) number.Value));
                default:
                    throw new NumericTypeException($"Invalid numeric type {number.Type}!");
            }
        }

        [BuiltInFunction("cosh", typeof(Func<Number, Number>), Namespace = "math")]
        public static Number Cosh(Number number)
        {
            number = number.RaiseTo(NumberType.Real);
            switch (number.Type)
            {
                case NumberType.Real:
                    return Real(Math.Cosh((double) number.Value));
                case NumberType.Complex:
                    return Complex(System.Numerics.Complex.Cosh((Complex) number.Value));
                default:
                    throw new NumericTypeException($"Invalid numeric type {number.Type}!");
            }
        }

        [BuiltInFunction("tanh", typeof(Func<Number, Number>), Namespace = "math")]
        public static Number Tanh(Number number)
        {
            number = number.RaiseTo(NumberType.Real);
            switch (number.Type)
            {
                case NumberType.Real:
                    return Real(Math.Tanh((double) number.Value));
                case NumberType.Complex:
                    return Complex(System.Numerics.Complex.Tanh((Complex) number.Value));
                default:
                    throw new NumericTypeException($"Invalid numeric type {number.Type}!");
            }
        }

        [BuiltInFunction("atan2", typeof(Func<Number, Number>), Namespace = "math")]
        public static Number Atan2(Number y, Number x)
        {
            if (!y.IsReal)
                throw new ArgumentOutOfRangeException(nameof(y), "Argument must be real!");
            if (!x.IsReal)
                throw new ArgumentOutOfRangeException(nameof(x), "Argument must be real!");
            y = y.CoerceTo(NumberType.Real);
            x = x.CoerceTo(NumberType.Real);
            return Real(Math.Atan2((double) y.Value, (double) x.Value));
        }

        [BuiltInFunction("log", typeof(Func<Number, Number>), Namespace = "math")]
        public static Number Log(Number number)
        {
            number = number.RaiseTo(NumberType.Real);
            if (number.IsReal && number < Zero)
                number = number.Raise();
            switch (number.Type)
            {
                case NumberType.NativeInt:
                case NumberType.Integer:
                case NumberType.Rational:
                    throw new NumericTypeException($"Failed to coerce {number}!");
                case NumberType.Real:
                    return Real(Math.Log((double) number.Value));
                case NumberType.Complex:
                    return Complex(System.Numerics.Complex.Log((Complex) number.Value));
                default:
                    throw new NumericTypeException($"Invalid numeric type {number.Type}!");
            }
        }

        [BuiltInFunction("logb", typeof(Func<Number, Number>), Namespace = "math")]
        public static Number LogB(Number number, Number b)
        {
            number = number.RaiseTo(NumberType.Real);
            if (number.IsReal && number < Zero)
                number = number.Raise();
            b = b.CoerceTo(NumberType.Real);
            switch (number.Type)
            {
                case NumberType.NativeInt:
                case NumberType.Integer:
                case NumberType.Rational:
                    throw new NumericTypeException($"Failed to coerce {number}!");
                case NumberType.Real:
                    return Real(Math.Log((double) number.Value, (double) b.Value));
                case NumberType.Complex:
                    return Complex(System.Numerics.Complex.Log((Complex) number.Value, (double) b.Value));
                default:
                    throw new NumericTypeException($"Invalid numeric type {number.Type}!");
            }
        }

        [BuiltInFunction("sqrt", typeof(Func<Number, Number>), Namespace = "math")]
        public static Number Sqrt(Number number)
        {
            number = number.RaiseTo(NumberType.Real);
            if (number.IsReal && number < Zero)
                number = number.Raise();
            switch (number.Type)
            {
                case NumberType.NativeInt:
                case NumberType.Integer:
                case NumberType.Rational:
                    throw new NumericTypeException($"Failed to coerce {number}!");
                case NumberType.Real:
                    return Real(Math.Sqrt((double) number.Value));
                case NumberType.Complex:
                    return Complex(System.Numerics.Complex.Sqrt((Complex) number.Value));
                default:
                    throw new NumericTypeException($"Invalid numeric type {number.Type}!");
            }
        }

        [BuiltInFunction("reciprocal", typeof(Func<Number, Number>), Namespace = "math")]
        public static Number Reciprocal(Number number) => NativeInt(1)/number;

        [BuiltInFunction("numerator", typeof(Func<Number, Number>), Namespace = "math")]
        public static Number Numerator(Number number)
        {
            if (!number.IsRational)
                throw new ArgumentOutOfRangeException(nameof(number), "Argument must be rational!");
            number = number.LowerTo(NumberType.Rational);
            switch (number.Type)
            {
                case NumberType.NativeInt:
                case NumberType.Integer:
                    return number;
                case NumberType.Rational:
                    return Integer(((BigRational) number.Value).Numerator);
                case NumberType.Real:
                case NumberType.Complex:
                    throw new NumericTypeException($"Failed to coerce {number}!");
                default:
                    throw new NumericTypeException($"Invalid numeric type {number.Type}!");
            }
        }

        [BuiltInFunction("denominator", typeof(Func<Number, Number>), Namespace = "math")]
        public static Number Denominator(Number number)
        {
            if (!number.IsRational)
                throw new ArgumentOutOfRangeException(nameof(number), "Argument must be rational!");
            number = number.LowerTo(NumberType.Rational);
            switch (number.Type)
            {
                case NumberType.NativeInt:
                case NumberType.Integer:
                    return NativeInt(1);
                case NumberType.Rational:
                    return Integer(((BigRational) number.Value).Denominator);
                case NumberType.Real:
                case NumberType.Complex:
                    throw new NumericTypeException($"Failed to coerce {number}!");
                default:
                    throw new NumericTypeException($"Invalid numeric type {number.Type}!");
            }
        }

        [BuiltInFunction("floor", typeof(Func<Number, Number>), Namespace = "math")]
        public static Number Floor(Number number)
        {
            if (!number.IsReal)
                throw new ArgumentOutOfRangeException(nameof(number), "Argument must be real!");
            number = number.LowerTo(NumberType.Real);
            switch (number.Type)
            {
                case NumberType.NativeInt:
                case NumberType.Integer:
                    return number;
                case NumberType.Rational:
                    return Integer(((BigRational) number.Value).GetWholePart() - (IsNegative(number) ? 1 : 0));
                case NumberType.Real:
                    return Integer((BigInteger) Math.Floor((double) number.Value));
                case NumberType.Complex:
                    throw new NumericTypeException($"Failed to coerce {number}!");
                default:
                    throw new NumericTypeException($"Invalid numeric type {number.Type}!");
            }
        }

        [BuiltInFunction("ceiling", typeof(Func<Number, Number>), Namespace = "math")]
        public static Number Ceiling(Number number)
        {
            if (!number.IsReal)
                throw new ArgumentOutOfRangeException(nameof(number), "Argument must be real!");
            number = number.LowerTo(NumberType.Real);
            switch (number.Type)
            {
                case NumberType.NativeInt:
                case NumberType.Integer:
                    return number;
                case NumberType.Rational:
                    return Integer(((BigRational) number.Value).GetWholePart() + (IsNegative(number) ? 0 : 1));
                case NumberType.Real:
                    return Integer((BigInteger) Math.Ceiling((double) number.Value));
                case NumberType.Complex:
                    throw new NumericTypeException($"Failed to coerce {number}!");
                default:
                    throw new NumericTypeException($"Invalid numeric type {number.Type}!");
            }
        }

        [BuiltInFunction("truncate", typeof(Func<Number, Number>), Namespace = "math")]
        public static Number Truncate(Number number)
        {
            if (!number.IsReal)
                throw new ArgumentOutOfRangeException(nameof(number), "Argument must be real!");
            number = number.LowerTo(NumberType.Real);
            switch (number.Type)
            {
                case NumberType.NativeInt:
                case NumberType.Integer:
                    return number;
                case NumberType.Rational:
                    return Integer(((BigRational) number.Value).GetWholePart());
                case NumberType.Real:
                    return Integer((BigInteger) Math.Truncate((double) number.Value));
                case NumberType.Complex:
                    throw new NumericTypeException($"Failed to coerce {number}!");
                default:
                    throw new NumericTypeException($"Invalid numeric type {number.Type}!");
            }
        }

        [BuiltInFunction("round", typeof(Func<Number, Number>), Namespace = "math")]
        public static Number Round(Number number)
        {
            if (!number.IsReal)
                throw new ArgumentOutOfRangeException(nameof(number), "Argument must be real!");
            number = number.LowerTo(NumberType.Real);
            switch (number.Type)
            {
                case NumberType.NativeInt:
                case NumberType.Integer:
                    return number;
                case NumberType.Rational:
                    return RoundRational(number);
                case NumberType.Real:
                    return Integer((BigInteger) Math.Round((double) number.Value, MidpointRounding.ToEven));
                case NumberType.Complex:
                    throw new NumericTypeException($"Failed to coerce {number}!");
                default:
                    throw new NumericTypeException($"Invalid numeric type {number.Type}!");
            }
        }

        [BuiltInFunction("fractional", typeof(Func<Number, Number>), Namespace = "math")]
        public static Number Fractional(Number number)
        {
            if (!number.IsReal)
                throw new ArgumentOutOfRangeException(nameof(number), "Argument must be real!");
            number = number.LowerTo(NumberType.Real);
            switch (number.Type)
            {
                case NumberType.NativeInt:
                case NumberType.Integer:
                    return Zero;
                case NumberType.Rational:
                    return Rational(((BigRational) number.Value).GetFractionPart());
                case NumberType.Real:
                    var val = (double) number.Value;
                    return Real(val - Math.Truncate(val));
                case NumberType.Complex:
                    throw new NumericTypeException($"Failed to coerce {number}!");
                default:
                    throw new NumericTypeException($"Invalid numeric type {number.Type}!");
            }
        }

        [BuiltInFunction("complex", typeof(Func<Number, Number>), Namespace = "math")]
        public static Number Complex(Number number) => number.CoerceTo(NumberType.Complex);

        [BuiltInFunction("make-rectangular", typeof(Func<Number, Number, Number>), Namespace = "math")]
        public static Number MakeRectangular(Number real, Number imaginary)
        {
            if (!real.IsReal)
                throw new ArgumentOutOfRangeException(nameof(real), "Argument must be real!");
            if (!imaginary.IsReal)
                throw new ArgumentOutOfRangeException(nameof(imaginary), "Argument must be real!");
            real = real.CoerceTo(NumberType.Real);
            imaginary = imaginary.CoerceTo(NumberType.Real);
            return Complex(new Complex((double) real.Value, (double) imaginary.Value));
        }

        [BuiltInFunction("make-polar", typeof(Func<Number, Number, Number>), Namespace = "math")]
        public static Number MakePolar(Number magnitude, Number angle)
        {
            if (!magnitude.IsReal)
                throw new ArgumentOutOfRangeException(nameof(magnitude), "Argument must be real!");
            if (!angle.IsReal)
                throw new ArgumentOutOfRangeException(nameof(angle), "Argument must be real!");
            magnitude = magnitude.CoerceTo(NumberType.Real);
            angle = angle.CoerceTo(NumberType.Real);
            return Complex(System.Numerics.Complex.FromPolarCoordinates((double) magnitude.Value, (double) angle.Value));
        }

        [BuiltInFunction("real-part", typeof(Func<Number, Number>), Namespace = "math")]
        public static Number RealPart(Number number) => Real(((Complex) number.CoerceTo(NumberType.Complex).Value).Real);

        [BuiltInFunction("imag-part", typeof(Func<Number, Number>), Namespace = "math")]
        public static Number ImagPart(Number number) => Real(((Complex) number.CoerceTo(NumberType.Complex).Value).Imaginary);

        [BuiltInFunction("magnitude", typeof(Func<Number, Number>), Namespace = "math")]
        public static Number Magnitude(Number number) => Real(((Complex) number.CoerceTo(NumberType.Complex).Value).Magnitude);

        [BuiltInFunction("angle", typeof(Func<Number, Number>), Namespace = "math")]
        public static Number Angle(Number number) => Real(((Complex) number.CoerceTo(NumberType.Complex).Value).Phase);

        #endregion
    }
}