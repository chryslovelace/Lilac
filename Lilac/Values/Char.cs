using System;
using System.Linq;
using System.Text;
using Lilac.Attributes;
using Lilac.Utilities;

namespace Lilac.Values
{
    public class Char : Value, IEquatable<Char>, IComparable<Char>, IComparable
    {
        public Char(byte[] bytes)
        {
            Bytes = bytes;
        }

        public Char(int value)
        {
            if (value < 0 || value > 0x10ffff || (value >= 0xd800 && value <= 0xdfff))
                throw new ArgumentOutOfRangeException(nameof(value), value,
                    "A valid UTF32 value is between 0x000000 and 0x10ffff, inclusive, and should not include surrogate codepoint values (0x00d800 ~ 0x00dfff).");
            Bytes = Encoding.Convert(Encoding.UTF32, Encoding.UTF8, BitConverter.GetBytes(value));
        }

        public Char(string value)
        {
            Bytes = Encoding.UTF8.GetBytes(value);
        }

        public byte[] Bytes { get; private set; }

        public bool Equals(Char other)
        {
            return CompareTo(other) == 0;
        }

        public int CompareTo(Char other)
        {
            if (ReferenceEquals(null, other)) return 1;
            if (ReferenceEquals(other, this)) return 0;
            return string.CompareOrdinal(ToString(), other.ToString());
        }

        public override Value GetMember(string name) => MemberContainer<Char>.GetMember(this, name);

        public override string ToString()
        {
            return Encoding.UTF8.GetString(Bytes);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Char);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as Char);
        }

        public override int CompareTo(Value other)
        {
            if (!(other is Char))
                throw new Exception($"Cannot compare char to {other.GetType().Name.CamelCaseToKebabCase()}!");
            return CompareTo((Char)other);
        }
        
        [BuiltInFunction("char", typeof(Func<Value, Char>))]
        public static Char ToChar(Value val)
        {
            var str = val as String;
            if (str != null)
            {
                if (str.CharIndices.Length != 1)
                    throw new Exception("Cannot convert string with length > 1 to char!");
                return new Char(str.Bytes);
            }
            var num = val as Number;
            if (num != null)
            {
                return new Char(num.AsInt32());
            }
            throw new Exception("Value must be a string or a number!");
        }

        [BuiltInFunction("char->utf8", typeof(Func<Char,List>))]
        public static List CharToUtf8(Char c)
        {
            return new List(c.Bytes.Select(Number.NativeInt));
        }

        [BuiltInFunction("char->utf16", typeof(Func<Char,List>))]
        public static List CharToUtf16(Char c)
        {
            var utf16Bytes = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, c.Bytes);
            var shorts = new short[utf16Bytes.Length/2];
            for (var i = 0; i < shorts.Length; i++)
            {
                shorts[i] = BitConverter.ToInt16(utf16Bytes, 2*i);
            }

            return new List(shorts.Select(Number.NativeInt));
        }

        [BuiltInFunction("char->utf32", typeof(Func<Char,List>))]
        public static List CharToUtf32(Char c)
        {
            var utf32Bytes = Encoding.Convert(Encoding.UTF8, Encoding.UTF32, c.Bytes);
            var ints = new int[utf32Bytes.Length / 4];
            for (var i = 0; i < ints.Length; i++)
            {
                ints[i] = BitConverter.ToInt32(utf32Bytes, 4 * i);
            }

            return new List(ints.Select(Number.NativeInt));
        }
    }
}