using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;

namespace SexyReact
{
    public interface IRxList<T> : IList<T>, IRxReadOnlyList<T>, IDisposable
    {
        void Move(int fromIndex, int toIndex);
        void Move(int toIndex, T item);
        void AddRange(IEnumerable<T> items);
        void InsertRange(IEnumerable<RxListItem<T>> items);
        void RemoveRange(IEnumerable<T> items);
        void RemoveRange(int startIndex, int count);
        void ModifyRange(IEnumerable<RxListItem<T>> items);
        void MoveRange(IEnumerable<RxListMovedItem<T>> items);
    }
}
