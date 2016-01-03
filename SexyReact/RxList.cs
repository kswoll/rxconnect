using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using SexyReact.Utils;
using System.Reactive;

namespace SexyReact
{
    /// <summary>
    /// A list that provides a number of observables for keeping track of its contents.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the list</typeparam>
    public class RxList<T> : IRxList<T>, IRxReadOnlyList<T>, INotifyCollectionChanged
    {
        private List<T> storage;
        private Lazy<Subject<IEnumerable<RxListItem<T>>>> rangeAdded = new Lazy<Subject<IEnumerable<RxListItem<T>>>>();
        private Lazy<Subject<IEnumerable<RxListItem<T>>>> rangeRemoved = new Lazy<Subject<IEnumerable<RxListItem<T>>>>();
        private Lazy<Subject<IEnumerable<RxListModifiedItem<T>>>> rangeModified = new Lazy<Subject<IEnumerable<RxListModifiedItem<T>>>>();
        private Lazy<Subject<RxListMovedItem<T>>> moved = new Lazy<Subject<RxListMovedItem<T>>>();
        private Lazy<Subject<RxListChange<T>>> changed = new Lazy<Subject<RxListChange<T>>>();
        private Lazy<Subject<Unit>> disposed = new Lazy<Subject<Unit>>();
        private NotifyCollectionChangedEventHandler collectionChanged;

        public RxList()
        {
            storage = new List<T>();
        }

        public RxList(IEnumerable<T> items)
        {
            storage = new List<T>(items);
        }

        public RxList(params T[] items) : this((IEnumerable<T>)items)
        {
            storage = new List<T>(items);
        }

        public void Dispose()
        {
            if (rangeAdded.IsValueCreated)
                rangeAdded.Value.Dispose();
            if (rangeRemoved.IsValueCreated)
                rangeRemoved.Value.Dispose();
            if (moved.IsValueCreated)
                moved.Value.Dispose();
            if (rangeModified.IsValueCreated)
                rangeModified.Value.Dispose();
            if (changed.IsValueCreated)
                changed.Value.Dispose();
            if (disposed.IsValueCreated)
            {
                disposed.Value.OnNext(default(Unit));
                disposed.Value.Dispose();
            }
        }

        public int Count => storage.Count;
        public bool IsReadOnly => false;
        public int IndexOf(T item) => storage.IndexOf(item);
        public IObservable<Unit> Disposed => disposed.Value;
        public IObservable<IEnumerable<RxListItem<T>>> RangeAdded => rangeAdded.Value;
        public IObservable<IEnumerable<RxListItem<T>>> RangeRemoved => rangeRemoved.Value;
        public IObservable<IEnumerable<RxListModifiedItem<T>>> RangeModified => rangeModified.Value;
        public IObservable<RxListChange<T>> Changed => changed.Value;
        public IObservable<RxListItem<T>> Added => RangeAdded.SelectMany(x => x);
        public IObservable<RxListItem<T>> Removed => RangeRemoved.SelectMany(x => x);
        public IObservable<RxListMovedItem<T>> Moved => moved.Value;
        public IObservable<RxListModifiedItem<T>> Modified => RangeModified.SelectMany(x => x);
        public IObservable<T> ItemAdded => Added.Select(x => x.Value);
        public IObservable<T> ItemRemoved => Removed.Select(x => x.Value);
        public IObservable<T> ItemMoved => Moved.Select(x => x.Value);
        public IObservable<T> ItemModified => Modified.Select(x => x.NewValue);
        public IObservable<IEnumerable<T>> ItemsAdded => RangeAdded.Select(x => x.Select(y => y.Value));
        public IObservable<IEnumerable<T>> ItemsRemoved => RangeRemoved.Select(x => x.Select(y => y.Value));
        public IObservable<IEnumerable<T>> ItemsModified => RangeModified.Select(x => x.Select(y => y.NewValue));
        public IEnumerator<T> GetEnumerator() => storage.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        protected virtual void OnChanged(RxListChange<T> change)
        {
            if (rangeAdded.IsValueCreated && change.Added.Any())
            {
                rangeAdded.Value.OnNext(change.Added);
            }
            if (rangeRemoved.IsValueCreated && change.Removed.Any())
            {
                rangeRemoved.Value.OnNext(change.Removed);
            }
            if (moved.IsValueCreated && change.Moved.HasValue)
            {
                moved.Value.OnNext(change.Moved.Value);
            }
            if (rangeModified.IsValueCreated && change.Modified.Any())
            {
                rangeModified.Value.OnNext(change.Modified);
            }
            if (changed.IsValueCreated)
            {
                changed.Value.OnNext(change);
            }
            if (collectionChanged != null)
            {
                if (change.Added.Any())
                {
                    collectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, change.Added.Select(x => x.Value).ToList(), change.Added.Select(x => x.Index).Min()));
                }
                if (change.Removed.Any())
                {
                    collectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, change.Removed.Select(x => x.Value).ToList(), change.Removed.Select(x => x.Index).Min()));
                }
                if (change.Modified.Any())
                {
                    collectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, change.Modified.Select(x => x.NewValue).ToList(), change.Modified.Select(x => x.OldValue).ToList(), change.Modified.Select(x => x.Index).Min()));
                }
                if (change.Moved != null)
                {
                    collectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, change.Moved.Value.Value, change.Moved.Value.ToIndex, change.Moved.Value.FromIndex));
                }
            }
        }

        public void Add(T item)
        {
            var index = storage.Count;
            storage.Add(item);
            OnChanged(new RxListChange<T>(added: Enumerables.Return(new RxListItem<T>(index, item))));
        }

        public void AddRange(IEnumerable<T> items)
        {
            var itemsArray = items.ToArray();
            var initialIndex = storage.Count;
            storage.AddRange(itemsArray);
            OnChanged(new RxListChange<T>(added: items.Select((x, i) => new RxListItem<T>(initialIndex + i, x))));
        }

        public void InsertRange(IEnumerable<RxListItem<T>> items)
        {
            var itemsArray = items.ToArray();
            foreach (var item in itemsArray.OrderByDescending(x => x.Index))
            {
                storage.Insert(item.Index, item.Value);
            }
            OnChanged(new RxListChange<T>(added: itemsArray));
        }

        public void RemoveRange(IEnumerable<T> items)
        {
            var itemsArray = items.Select(x => new RxListItem<T>(storage.IndexOf(x), x)).ToArray();
            foreach (var item in itemsArray.OrderByDescending(x => x.Index))
                storage.RemoveAt(item.Index);
            OnChanged(new RxListChange<T>(removed: itemsArray));
        }

        public void RemoveRange(int startIndex, int count)
        {
            var itemsArray = storage.Select((x, i) => new RxListItem<T>(startIndex + i, storage[startIndex + i]));
            foreach (var item in itemsArray.OrderByDescending(x => x.Index))
                storage.RemoveAt(item.Index);
            OnChanged(new RxListChange<T>(removed: itemsArray));
        }

        public void ModifyRange(IEnumerable<RxListItem<T>> items)
        {
            var itemsArray = items.ToArray();
            var modified = new List<RxListModifiedItem<T>>();
            foreach (var item in itemsArray)
            {
                var oldValue = storage[item.Index];
                storage[item.Index] = item.Value;
                modified.Add(new RxListModifiedItem<T>(item.Index, oldValue, item.Value));
            }
            OnChanged(new RxListChange<T>(modified: modified));
        }

        public void Clear()
        {
            var change = new RxListChange<T>(removed: storage.Select((x, i) => new RxListItem<T>(i, x)).ToArray());
            storage.Clear();
            OnChanged(change);
        }

        public bool Contains(T item)
        {
            return storage.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            storage.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            var index = storage.IndexOf(item);
            if (index == -1)
                return false;

            storage.RemoveAt(index);
            OnChanged(new RxListChange<T>(removed: Enumerables.Return(new RxListItem<T>(index, item))));
            return true;
        }

        public void Insert(int index, T item)
        {
            storage.Insert(index, item);
            OnChanged(new RxListChange<T>(added: Enumerables.Return(new RxListItem<T>(index, item))));
        }

        public void RemoveAt(int index)
        {
            var item = storage[index];
            storage.RemoveAt(index);
            OnChanged(new RxListChange<T>(removed: Enumerables.Return(new RxListItem<T>(index, item))));
        }

        public T this[int index]
        {
            get
            {
                return storage[index];
            }
            set
            {
                var oldValue = storage[index];
                storage[index] = value;
                OnChanged(new RxListChange<T>(modified: Enumerables.Return(new RxListModifiedItem<T>(index, oldValue, value))));
            }
        }

        public void Move(int fromIndex, int toIndex)
        {
            var item = storage[fromIndex];
            storage.RemoveAt(fromIndex);
            storage.Insert(toIndex, item);
            OnChanged(new RxListChange<T>(moved: new RxListMovedItem<T>(fromIndex, toIndex, item)));
        }

        public void Move(int toIndex, T item)
        {
            var fromIndex = storage.IndexOf(item);
            Move(fromIndex, toIndex);
        }

        event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
        {
            add { collectionChanged = (NotifyCollectionChangedEventHandler)Delegate.Combine(collectionChanged, value); }
            remove { collectionChanged = (NotifyCollectionChangedEventHandler)Delegate.Remove(collectionChanged, value); }
        }
    }
}