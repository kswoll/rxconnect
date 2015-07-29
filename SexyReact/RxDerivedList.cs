using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive;

namespace SexyReact
{
    public class RxDerivedList<T> : IRxReadOnlyList<T>
    {
        private RxList<T> storage;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<T> GetEnumerator() => storage.GetEnumerator();
        public int Count => storage.Count;
        public T this[int index] => storage[index];
        public IObservable<Unit> Disposed => storage.Disposed;
        public IObservable<IEnumerable<RxListItem<T>>> RangeAdded => storage.RangeAdded;
        public IObservable<IEnumerable<RxListItem<T>>> RangeRemoved => storage.RangeRemoved;
        public IObservable<IEnumerable<RxListModifiedItem<T>>> RangeModified => storage.RangeModified;
        public IObservable<RxListChange<T>> Changed => storage.Changed;
        public IObservable<RxListItem<T>> Added => storage.Added;
        public IObservable<RxListItem<T>> Removed => storage.Removed;
        public IObservable<RxListMovedItem<T>> Moved => storage.Moved;
        public IObservable<RxListModifiedItem<T>> Modified => storage.Modified;
        public IObservable<T> ItemAdded => storage.ItemAdded;
        public IObservable<T> ItemRemoved => storage.ItemRemoved;
        public IObservable<T> ItemMoved => storage.ItemMoved;
        public IObservable<T> ItemModified => storage.ItemModified;
        public IObservable<IEnumerable<T>> ItemsAdded => storage.ItemsAdded;
        public IObservable<IEnumerable<T>> ItemsRemoved => storage.ItemsRemoved;
        public IObservable<IEnumerable<T>> ItemsModified => storage.ItemsModified;

        public RxDerivedList(RxList<T> storage)
        {
            this.storage = storage;
        }
    }
}
