using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using RxConnect.Utils;

namespace RxConnect
{
    /// <summary>
    /// A list that provides a number of observables for keeping track of its contents.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the list</typeparam>
    public class RxList<T> : IList<T>, IDisposable
    {
        private List<T> storage;
        private Lazy<Subject<IEnumerable<RxListItem<T>>>> rangeAdded = new Lazy<Subject<IEnumerable<RxListItem<T>>>>();
        private Lazy<Subject<IEnumerable<RxListItem<T>>>> rangeRemoved = new Lazy<Subject<IEnumerable<RxListItem<T>>>>();
        private Lazy<Subject<IEnumerable<RxListMovedItem<T>>>> rangeMoved = new Lazy<Subject<IEnumerable<RxListMovedItem<T>>>>();
        private Lazy<Subject<IEnumerable<RxListItem<T>>>> rangeModified = new Lazy<Subject<IEnumerable<RxListItem<T>>>>();
        private Lazy<Subject<RxListChange<T>>> changed = new Lazy<Subject<RxListChange<T>>>();

        public RxList()
        {
            storage = new List<T>();
        }

        public RxList(IEnumerable<T> items)
        {
            storage = new List<T>(items);
        }

        public void Dispose()
        {
            if (rangeAdded.IsValueCreated)
                rangeAdded.Value.Dispose();
            if (rangeRemoved.IsValueCreated)
                rangeRemoved.Value.Dispose();
            if (rangeMoved.IsValueCreated)
                rangeMoved.Value.Dispose();
            if (rangeModified.IsValueCreated)
                rangeModified.Value.Dispose();
            if (changed.IsValueCreated)
                changed.Value.Dispose();
        }

        public IObservable<IEnumerable<RxListItem<T>>> RangeAdded
        {
            get { return rangeAdded.Value; }
        }

        public IObservable<IEnumerable<RxListItem<T>>> RangeRemoved
        {
            get { return rangeRemoved.Value; }
        }

        public IObservable<IEnumerable<RxListMovedItem<T>>> RangeMoved
        {
            get { return rangeMoved.Value; }
        }

        public IObservable<IEnumerable<RxListItem<T>>> RangeModified
        {
            get { return rangeModified.Value; }
        }

        public IObservable<RxListChange<T>> Changed
        {
            get { return changed.Value; }
        }

        public IObservable<RxListItem<T>> Added
        {
            get { return RangeAdded.SelectMany(x => x); }
        }

        public IObservable<RxListItem<T>> Removed
        {
            get { return RangeRemoved.SelectMany(x => x); }
        }

        public IObservable<RxListMovedItem<T>> Moved
        {
            get { return RangeMoved.SelectMany(x => x); }
        }

        public IObservable<RxListItem<T>> Modified
        {
            get { return RangeModified.SelectMany(x => x); }
        }

        public IObservable<T> ItemAdded
        {
            get { return Added.Select(x => x.Value); }
        }

        public IObservable<T> ItemRemoved
        {
            get { return Removed.Select(x => x.Value); }
        }

        public IObservable<T> ItemMoved
        {
            get { return Moved.Select(x => x.Value); }
        }

        public IObservable<T> ItemModified
        {
            get { return Modified.Select(x => x.Value); }
        }

        public IObservable<IEnumerable<T>> ItemsAdded
        {
            get { return RangeAdded.Select(x => x.Select(y => y.Value)); }
        }

        public IObservable<IEnumerable<T>> ItemsRemoved
        {
            get { return RangeRemoved.Select(x => x.Select(y => y.Value)); }
        }

        public IObservable<IEnumerable<T>> ItemsMoved
        {
            get { return RangeMoved.Select(x => x.Select(y => y.Value)); }
        }

        public IObservable<IEnumerable<T>> ItemsModified
        {
            get { return RangeModified.Select(x => x.Select(y => y.Value)); }
        }

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
            if (rangeMoved.IsValueCreated && change.Moved.Any())
            {
                rangeMoved.Value.OnNext(change.Moved);
            }
            if (rangeModified.IsValueCreated && change.Modified.Any())
            {
                rangeModified.Value.OnNext(change.Modified);
            }
            if (changed.IsValueCreated)
            {
                changed.Value.OnNext(change);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return storage.GetEnumerator();
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

        public int Count
        {
            get { return storage.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public int IndexOf(T item)
        {
            return storage.IndexOf(item);
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
                storage[index] = value;
                OnChanged(new RxListChange<T>(modified: Enumerables.Return(new RxListItem<T>(index, value))));
            }
        }

        public void Move(int fromIndex, int toIndex)
        {
            var item = storage[fromIndex];
            storage.RemoveAt(fromIndex);
            storage.Insert(toIndex, item);
            OnChanged(new RxListChange<T>(moved: Enumerables.Return(new RxListMovedItem<T>(fromIndex, toIndex, item))));
        }
    }
}
