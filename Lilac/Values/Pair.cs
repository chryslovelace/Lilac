using System;
using System.Collections.Generic;
using System.Text;
using Lilac.Attributes;
using Lilac.Utilities;

namespace Lilac.Values
{
    public class Pair : Value
    {
        public Pair(Value car, Value cdr)
        {
            CarValue = car;
            CdrValue = cdr;
        }

        

        [BuiltInMember("car")]
        public Value CarValue { get; set; }

        [BuiltInMember("cdr")]
        public Value CdrValue { get; set; }

        public override Value GetMember(string name) => MemberContainer<Pair>.GetMember(this, name);
        public override bool SetMember(string name, Value value) => MemberContainer<Pair>.SetMember(this, name, value);
        public override string ToString()
        {
            return IsLinkedList(this) ? $"({LinkedListString()})" : $"({CarValue} . {CdrValue})";
        }

        private string LinkedListString()
        {
            if (CdrValue is Unit) return CarValue.ToString();
            return $"{CarValue}; {((Pair) CdrValue).LinkedListString()}";
        }

        public static Value LinkedList(IReadOnlyList<Value> values)
        {
            Value current = Unit.Value;
            for (var i = values.Count - 1; i >= 0; i--)
            {
                current = new Pair(values[i], current);
            }
            return current;
        }

        [BuiltInFunction("list->linked-list", typeof(Func<List, Value>))]
        public static Value ListToLinkedList(List list)
        {
            return LinkedList(list.Values);
        }

        [BuiltInFunction("cons", typeof(Func<Value, Value, Pair>))]
        public static Pair Cons(Value car, Value cdr)
        {
            return new Pair(car, cdr);
        }

        [BuiltInFunction("car", typeof(Func<Pair, Value>))]
        public static Value Car(Pair pair)
        {
            return pair.CarValue;
        }

        [BuiltInFunction("cdr", typeof(Func<Pair, Value>))]
        public static Value Cdr(Pair pair)
        {
            return pair.CdrValue;
        }
    }
}