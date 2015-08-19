using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;

namespace SexyReact
{
    /// <summary>
    /// Use RxListExtensions' list.Derive(...)
    /// </summary>
    public class RxDerivedList<TSource, T> : IRxReadOnlyList<T>
    {
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
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        protected readonly RxList<T> storage;

        private IDisposable subscription;
        private object locker = new object();
        private Func<TSource, T> selector;
        private Action<T> removed;

        protected internal RxDerivedList(IRxList<TSource> source, Func<TSource, T> selector, Action<T> removed = null)
        {
            this.selector = selector;
            this.removed = removed;

            storage = new RxList<T>(source.Select(selector));

            lock (locker)
            {
                subscription = source.Changed.Subscribe(OnSourceChanged);
            }
        }

        protected virtual void OnSourceChanged(RxListChange<TSource> changes)
        {
            lock (locker)
            {
                OnInsert(changes.Added.ToArray());
                OnRemove(changes.Removed.OrderByDescending(x => x.Index).ToArray());
                OnModify(changes.Modified.ToArray());
                if (changes.Moved != null)
                {
                    OnMove(changes.Moved.Value);
                }                    
            }            
        }

        protected virtual void OnInsert(IEnumerable<RxListItem<TSource>> inserts)
        {
            storage.InsertRange(inserts.Select(x => new RxListItem<T>(x.Index, selector(x.Value))));
        }

        protected virtual void OnRemove(IEnumerable<RxListItem<TSource>> removes)
        {
            foreach (var item in removes)
            {
                removed?.Invoke(storage[item.Index]);
                storage.RemoveAt(item.Index);            
            }
        }

        protected virtual void OnModify(IEnumerable<RxListModifiedItem<TSource>> modifications)
        {
            foreach (var item in modifications)
            {
                removed?.Invoke(storage[item.Index]);
                storage[item.Index] = selector(item.NewValue);
            }
        }

        protected virtual void OnMove(RxListMovedItem<TSource> move)
        {
            storage.Move(move.FromIndex, move.ToIndex);            
        }

        public void Dispose()
        {
            storage.Dispose();
            subscription.Dispose();
        }
    }
}
