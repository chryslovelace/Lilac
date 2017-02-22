using System.Collections.Generic;

namespace Lilac.Utilities
{
    public interface IBidirectionalIterator<out T> : IEnumerator<T>
    {
        bool MovePrevious();
        IBidirectionalIterator<T> Copy();
    }
}