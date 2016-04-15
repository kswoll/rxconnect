using System;
using System.Collections.Generic;
using System.Reactive;

namespace SexyReact
{
    public interface IRxListObservables<T>
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

    public interface IRxListObservables
    {
        IObservable<Unit> Disposed { get; }
        IObservable<IEnumerable<RxListItem<object>>> RangeAdded { get; }
        IObservable<IEnumerable<RxListItem<object>>> RangeRemoved { get; }
        IObservable<IEnumerable<RxListModifiedItem<object>>> RangeModified { get; }
        IObservable<RxListChange<object>> Changed { get; }
        IObservable<RxListItem<object>> Added { get; }
        IObservable<RxListItem<object>> Removed { get; }
        IObservable<RxListMovedItem<object>> Moved { get; }
        IObservable<RxListModifiedItem<object>> Modified { get; }
        IObservable<object> ItemAdded { get; }
        IObservable<object> ItemRemoved { get; }
        IObservable<object> ItemMoved { get; }
        IObservable<object> ItemModified { get; }
        IObservable<IEnumerable<object>> ItemsAdded { get; }
        IObservable<IEnumerable<object>> ItemsRemoved { get; }
        IObservable<IEnumerable<object>> ItemsModified { get; }
    }
}
