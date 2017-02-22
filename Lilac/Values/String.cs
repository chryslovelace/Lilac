using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Lilac.Attributes;
using Lilac.Utilities;

// ReSharper disable AssignNullToNotNullAttribute

namespace Lilac.Values
{
    public class String : Value, IComparable<String>, IEquatable<String>, IComparable
    {
        #region Private Static Properties

        private static Dictionary<string, String> InternedConstructedStrings { get; } = new Dictionary<string, String>();
        private static Dictionary<string, String> InternedParsedStrings { get; } = new Dictionary<string, String>();

        private static IReadOnlyDictionary<char, char> EscapeSequences { get; } = new Dictionary<char, char>
        {
            {'\'', '\''},
            {'\"', '\"'},
            {'\\', '\\'},
            {'0', '\0'},
            {'a', '\a'},
            {'b', '\b'},
            {'f', '\f'},
            {'n', '\n'},
            {'r', '\r'},
            {'t', '\t'},
            {'v', '\v'}
        };

        #endregion

        #region Constructor


        #endregion

        #region Public Instance Properties

        public int[] CharIndices { get; private set; }
        public byte[] Bytes { get; private set; }

        public byte[] AsUtf16 => Encoding.Convert(Encoding.UTF8, Encoding.Unicode, Bytes);
        public byte[] AsUtf32 => Encoding.Convert(Encoding.UTF8, Encoding.UTF32, Bytes);
        
        [BuiltInMember("length", GetOnly = true)]
        public Number Length => Number.NativeInt(CharIndices.Length);

        [BuiltInMember("chars", GetOnly = true)]
        public List Chars
        {
            get
            {
                var chars = new Char[CharIndices.Length];
                for (var i = 0; i < CharIndices.Length; i++)
                {
                    var charStart = CharIndices[i];
                    var charEnd = i == CharIndices.Length - 1 ? Bytes.Length : CharIndices[i + 1];
                    var bytes = new byte[charEnd - charStart];
                    Array.Copy(Bytes, charStart, bytes, 0, charEnd - charStart);
                    chars[i] = new Char(bytes);
                }
                return new List(chars);
            }
        }
        
        #endregion

        #region Public Instance Methods

        public override Value GetMember(string name) => MemberContainer<String>.GetMember(this, name);

        public int CompareTo(String other)
        {
            if (ReferenceEquals(null, other)) return 1;
            if (ReferenceEquals(other, this)) return 0;
            return string.CompareOrdinal(ToString(), other.ToString());
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as String);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as String);
        }

        public override int CompareTo(Value other)
        {
            if (!(other is String))
                throw new Exception($"Cannot compare string to {other.GetType().Name.CamelCaseToKebabCase()}!");
            return CompareTo((String)other);
        }

        public bool Equals(String other)
        {
            return CompareTo(other) == 0;
        }

        public override string ToString()
        {
            return Encoding.UTF8.GetString(Bytes);
        }

        [BuiltInMethod("at", typeof(Func<Number, Char>))]
        public Char At(Number number)
        {
            if (!number.IsInteger)
                throw new Exception("Cannot index with non integral value!");
            if (number < Number.Zero || number >= Length)
                throw new Exception("Index out of range!");
            var index = number.AsInt32();
            var byteStart = CharIndices[index];
            var byteEnd = index == CharIndices.Length - 1 ? Bytes.Length : CharIndices[index + 1];
            var bytes = new byte[byteEnd - byteStart];
            Array.Copy(Bytes, byteStart, bytes, 0, byteEnd - byteStart);
            return new Char(bytes);
        }

        [BuiltInMethod("concat", typeof(Func<String, String>))]
        public String Concat(String other) => Concat(this, other);

        
        #endregion

        #region Public Static Methods

        public static String Get(string str)
        {
            String value;
            if (InternedConstructedStrings.TryGetValue(str, out value))
            {
                return value;
            }
            
            var bytes = new List<byte>();
            var charIndices = new List<int>();
            var tee = StringInfo.GetTextElementEnumerator(str);
            while (tee.MoveNext())
            {
                charIndices.Add(bytes.Count);
                bytes.AddRange(Encoding.UTF8.GetBytes(tee.GetTextElement()));
            }

            value = new String
            {
                Bytes = bytes.ToArray(),
                CharIndices = charIndices.ToArray()
            };
            InternedConstructedStrings.Add(str, value);
            return value;
        }

        public static String Parse(string str)
        {
            String value;
            if (InternedParsedStrings.TryGetValue(str, out value))
                return value;

            var sb = new StringBuilder();
            var substring = str.Substring(1, str.Length - 2);
            for (var i = 0; i < substring.Length; i++)
            {
                var c = substring[i];
                if (c != '\\')
                {
                    sb.Append(c);
                }
                else
                {
                    var e = substring[++i];
                    char escape;
                    if (EscapeSequences.TryGetValue(e, out escape))
                    {
                        sb.Append(escape);
                    }
                    else
                    {
                        int hex;
                        switch (e)
                        {
                            case 'x':
                                hex = int.Parse(substring.Substring(i + 1, 2), NumberStyles.HexNumber);
                                sb.Append((char) hex);
                                i += 2;
                                break;
                            case 'u':
                                hex = int.Parse(substring.Substring(i + 1, 4), NumberStyles.HexNumber);
                                sb.Append((char)hex);
                                i += 4;
                                break;
                            case 'U':
                                hex = int.Parse(substring.Substring(i + 1, 8), NumberStyles.HexNumber);
                                sb.Append(char.ConvertFromUtf32(hex));
                                i += 8;
                                break;
                            default:
                                throw new Exception($"Failed to parse string '{str}'!");
                        }
                    }
                }
            }

            var s = Get(sb.ToString());
            InternedParsedStrings[str] = s;
            return s;
        }

        [BuiltInFunction("++", typeof(Func<String, String, String>), IsOperator = true)]
        public static String Concat(String lhs, String rhs)
        {
            var bytes = lhs.Bytes.Concat(rhs.Bytes).ToArray();
            var charIndices = lhs.CharIndices.Concat(rhs.CharIndices.Select(ci => ci + lhs.Bytes.Length)).ToArray();

            return new String
            {
                Bytes = bytes,
                CharIndices = charIndices
            };
        }

        [BuiltInFunction("substring", typeof(Func<String, Number, Number, String>))]
        public static String Substring(String str, Number start, Number length)
        {
            var startIndex = start.AsInt32();
            if (startIndex < 0 || startIndex >= str.CharIndices.Length)
                throw new ArgumentOutOfRangeException(nameof(start));
            if (length < Number.Zero)
                throw new ArgumentOutOfRangeException(nameof(length));
            if (!Number.IsFinite(length).AsBool() || start + length > str.Length)
                throw new ArgumentOutOfRangeException(nameof(length));
            var len = length.AsInt32();
            var byteStart = str.CharIndices[startIndex];
            var byteLen = str.CharIndices[startIndex + len] - byteStart;
            var bytes = new byte[byteLen];
            Array.Copy(str.Bytes, byteStart, bytes, 0, byteLen);
            var charIndices = str.CharIndices.Skip(startIndex).Take(len).Select(i => i - byteStart).ToArray();

            return new String
            {
                Bytes = bytes,
                CharIndices = charIndices
            };
        }

        [BuiltInFunction("string->utf8", typeof(Func<String, List>))]
        public static List StringToUtf8(String str)
        {
            return new List(str.Bytes.Select(Number.NativeInt));
        }

        [BuiltInFunction("string->utf16", typeof(Func<String, List>))]
        public static List StringToUtf16(String str)
        {
            var utf16Bytes = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, str.Bytes);
            var shorts = new short[utf16Bytes.Length / 2];
            for (var i = 0; i < shorts.Length; i++)
            {
                shorts[i] = BitConverter.ToInt16(utf16Bytes, 2 * i);
            }

            return new List(shorts.Select(Number.NativeInt));
        }

        [BuiltInFunction("string->utf32", typeof(Func<String, List>))]
        public static List StringToUtf32(String str)
        {
            var utf32Bytes = Encoding.Convert(Encoding.UTF8, Encoding.UTF32, str.Bytes);
            var ints = new int[utf32Bytes.Length / 4];
            for (var i = 0; i < ints.Length; i++)
            {
                ints[i] = BitConverter.ToInt32(utf32Bytes, 4 * i);
            }

            return new List(ints.Select(Number.NativeInt));
        }



        #endregion
    }
}