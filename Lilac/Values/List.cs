using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Lilac.Attributes;
using Lilac.Utilities;

namespace Lilac.Values
{
    public class List : Value
    {
        private List() { }

        public List(IEnumerable<Value> values)
        {
            Values = values.ToImmutableList();
        }

        #region Public Instance Properties

        public ImmutableList<Value> Values { get; private set; }

        [BuiltInMember("length", GetOnly = true)]
        public Number Length => Number.NativeInt(Values.Count);

        [BuiltInMember("empty?", GetOnly = true)]
        public Boolean IsEmpty => Boolean.Get(Length == Number.Zero);

        [BuiltInMember("copy", GetOnly = true)]
        public List Copy => new List { Values = Values };

        #endregion

        #region Public Instance Methods

        public override string ToString()
        {
            return $"[{string.Join("; ", Values)}]";
        }

        public override Value GetMember(string name) => MemberContainer<List>.GetMember(this, name);

        [BuiltInMethod("at", typeof(Func<Number, Value>))]
        public Value At(Number number)
        {
            if (!number.IsInteger)
                throw new Exception("Cannot index with non integral value!");
            if (number < Number.Zero || number >= Length)
                throw new Exception("Index out of range!");
            var index = number.AsInt32();
            return Values[index];
        }

        [BuiltInMethod("add!", typeof(Func<Value, List>))]
        public List Add(Value value)
        {
            Values = Values.Add(value);
            return this;
        }

        [BuiltInMethod("append!", typeof(Func<List, List>))]
        public List Append(List value)
        {
            Values = Values.AddRange(value.Values);
            return this;
        }

        [BuiltInMethod("concat", typeof(Func<List, List>))]
        public List Concat(List value)
        {
            return new List(Values.Concat(value.Values));
        }

        [BuiltInMethod("skip", typeof(Func<Number, List>))]
        public List Skip(Number num)
        {
            return new List(Values.Skip(num.AsInt32()));
        }

        [BuiltInMethod("take", typeof(Func<Number, List>))]
        public List Take(Number num)
        {
            return new List(Values.Take(num.AsInt32()));
        }

        [BuiltInMethod("reverse", typeof(Func<List>))]
        public List Reverse()
        {
            return new List(Values.Reverse());
        }

        #endregion

        #region Public Static Methods

        [BuiltInFunction("linked-list->list", typeof(Func<Value, List>))]
        public static List LinkedListToList(Value linkedList)
        {
            if (!IsLinkedList(linkedList))
                throw new ArgumentException("Value is not a valid linked list!", nameof(linkedList));
            var list = new List<Value>();
            var current = linkedList as Pair;
            while (current != null)
            {
                list.Add(current.CarValue);
                current = current.CdrValue as Pair;
            }
            return new List(list);
        }

        #endregion

    }
}