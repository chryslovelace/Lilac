using System.Collections;
using System.Collections.Generic;

namespace Lilac.Utilities
{
    public class BidirectionalIterator<T> : IBidirectionalIterator<T>
    {
        private IEnumerator<T> Enumerator { get; set; }
        private List<T> List { get; set; }
        private int Index { get; set; }
        private object Lock { get; set; }
        
        public BidirectionalIterator(IEnumerable<T> source)
        {
            Enumerator = source.GetEnumerator();
            List = new List<T>();
            Index = -1;
            Lock = new object();
        }

        public T Current
        {
            get { lock (Lock) { return List[Index]; } }
        }

        object IEnumerator.Current
        {
            get { lock (Lock) { return Current; } }
        }

        public bool MoveNext()
        {
            lock (Lock)
            {
                if (Index == List.Count)
                    return false;
                ++Index;
                if (Index != List.Count)
                    return true;
                if (!Enumerator.MoveNext())
                    return false;
                List.Add(Enumerator.Current);
                return true;
            }
        }

        public bool MovePrevious()
        {
            if (Index > -1)
                --Index;
            return Index != -1;
        }

        public IBidirectionalIterator<T> Copy()
        {
            return (IBidirectionalIterator<T>) MemberwiseClone();
        }

        public void Dispose() => Enumerator.Dispose();

        public void Reset() => Index = -1;
    }
}