using System;
using System.Collections.Generic;
using System.Linq;

namespace Lilac.Utilities
{
    public static class LinkedListExtensions
    {
        public static IEnumerable<LinkedListNode<T>> Nodes<T>(this LinkedList<T> list)
        {
            var curr = list.First;
            while (curr != null)
            {
                yield return curr;
                curr = curr.Next; 
            }
        }

        public static LinkedListNode<T> Find<T>(this LinkedList<T> list, Func<T, bool> predicate)
        {
            return list.Nodes().FirstOrDefault(n => predicate(n.Value));
        }
        
    }
}