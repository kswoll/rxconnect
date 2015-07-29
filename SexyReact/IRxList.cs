using System.Collections.Generic;

namespace SexyReact
{
    public interface IRxList<T> : IList<T>, IRxListObservables<T>
    {
        void Move(int fromIndex, int toIndex);
        void Move(int toIndex, T item);
        void AddRange(IEnumerable<T> items);
        void InsertRange(IEnumerable<RxListItem<T>> items);
        void RemoveRange(IEnumerable<T> items);
        void RemoveRange(int startIndex, int count);
        void ModifyRange(IEnumerable<RxListItem<T>> items);
    }
}
