using System;
using System.Collections.Generic;
using System.Text;

namespace RxConnect
{
    public interface IRxList<T> : IList<T>, IDisposable
    {
        IObservable<IEnumerable<RxListItem<T>>> RangeAdded { get; }
        IObservable<IEnumerable<RxListItem<T>>> RangeRemoved { get; }
        IObservable<IEnumerable<RxListMovedItem<T>>> RangeMoved { get; }
        IObservable<IEnumerable<RxListModifiedItem<T>>> RangeModified { get; }
        IObservable<RxListChange<T>> Changed { get; }
        IObservable<RxListItem<T>> Added { get; }
        IObservable<RxListItem<T>> Removed { get; }
        IObservable<RxListMovedItem<T>> Moved { get; }
        IObservable<RxListModifiedItem<T>> Modified { get; }
        IObservable<T> ItemAdded { get; }
        IObservable<T> ItemRemoved { get; }
        IObservable<T> ItemMoved { get; }
        IObservable<T> ItemModified { get; }
        IObservable<IEnumerable<T>> ItemsAdded { get; }
        IObservable<IEnumerable<T>> ItemsRemoved { get; }
        IObservable<IEnumerable<T>> ItemsMoved { get; }
        IObservable<IEnumerable<T>> ItemsModified { get; }

        void Move(int fromIndex, int toIndex);
        void Move(int toIndex, T item);
        void AddRange(IEnumerable<T> items);
        void RemoveRange(IEnumerable<T> items);
        void RemoveRange(int startIndex, int count);
    }
}
