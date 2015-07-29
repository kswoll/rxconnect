using System;
using System.Collections.Generic;
using System.Reactive;

namespace SexyReact
{
    public interface IRxReadOnlyList<T> : IReadOnlyList<T>
    {
        IObservable<Unit> Disposed { get; }
        IObservable<IEnumerable<RxListItem<T>>> RangeAdded { get; }
        IObservable<IEnumerable<RxListItem<T>>> RangeRemoved { get; }
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
        IObservable<IEnumerable<T>> ItemsModified { get; }
    }
}
