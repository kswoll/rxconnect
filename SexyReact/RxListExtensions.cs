using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using SexyReact.Utils;

namespace SexyReact
{
    public static class RxListExtensions
    {
        /// <summary>
        /// Returns an observable that fires whenever a value for an element currently in the list changes.  This means that if
        /// an element was in the list when this method is called but subsequently removed, further changes to the property on 
        /// that element will not propagate down this observable.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the list</typeparam>
        /// <typeparam name="TValue">The type of the value returned by the selector</typeparam>
        /// <param name="list">The list on which to observe elements</param>
        /// <param name="selector">Identifies a property path to a value on the current element</param>
        /// <returns>The observable that monitors changes to a property for elements currently in the list</returns>
        public static IObservable<RxListObservedElement<T, TValue>> ObserveElementPropertyChanged<T, TValue>(this IRxList<T> list, Expression<Func<T, TValue>> selector)
            where T : IRxObject
        {
            return new RxListElementObservable<T, TValue>(list, selector, true);
        }

        /// <summary>
        /// Returns an observable that fires whenever a value for an element currently in the list changes.  This means that if
        /// an element was in the list when this method is called but subsequently removed, further changes to the property on 
        /// that element will not propagate down this observable.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the list</typeparam>
        /// <typeparam name="TValue">The type of the value returned by the selector</typeparam>
        /// <param name="list">The list on which to observe elements</param>
        /// <param name="selector">Identifies a property path to a value on the current element</param>
        /// <returns>The observable that monitors changes to a property for elements currently in the list</returns>
        public static IObservable<RxListObservedElement<T, TValue>> ObserveElementProperty<T, TValue>(this IRxList<T> list, Expression<Func<T, TValue>> selector)
            where T : IRxObject
        {
            return new RxListElementObservable<T, TValue>(list, selector, false);
        }

        /// <summary>
        /// Observe the values of multiple properties. Each time the value of one of the properties changes, a new item
        /// will be emitted.  The value is based on resultSelector which has access to the current value of each property.
        /// </summary>
        public static IObservable<RxListObservedElement<T, Unit>> ObserveElementChange<T>(
            this IRxList<T> list
        )
            where T : IRxObject
        {
            return new RxListElementObservable<T, Unit>(list, null, true);
        }

        /// <summary>
        /// Observe the values of multiple properties. Each time the value of one of the properties changes, a new item
        /// will be emitted.  The value is based on resultSelector which has access to the current value of each property.
        /// </summary>
        public static IObservable<RxListObservedElement<T, TValue>> ObserveElementChange<T, TValue1, TValue2, TValue>(
            this IRxList<T> list, 
            Expression<Func<T, TValue1>> property1,
            Expression<Func<T, TValue2>> property2,
            Func<TValue1, TValue2, TValue> resultSelector
        )
            where T : IRxObject
        {
            var property1Observable = list.ObserveElementPropertyChanged(property1);
            var property2Observable = list.ObserveElementPropertyChanged(property2);
            return property1Observable.CombineLatest(property2Observable, (x, y) => new RxListObservedElement<T, TValue>(x.Element, resultSelector(x.Value, y.Value)));
        }

        /// <summary>
        /// Observe the values of multiple properties. Each time the value of one of the properties changes, a new item
        /// will be emitted.  The value is based on resultSelector which has access to the current value of each property.
        /// </summary>
        public static IObservable<RxListObservedElement<T, Unit>> ObserveElementChange<T, TValue1, TValue2>(
            this IRxList<T> list, 
            Expression<Func<T, TValue1>> property1,
            Expression<Func<T, TValue2>> property2
        )
            where T : IRxObject
        {
            return list.ObserveElementChange(property1, property2, (x1, x2) => Unit.Default);
        }

        /// <summary>
        /// Observe the values of multiple properties. Each time the value of one of the properties changes, a new item
        /// will be emitted.  The value is based on resultSelector which has access to the current value of each property.
        /// </summary>
        public static IObservable<RxListObservedElement<T, Unit>> ObserveElement<T, TValue1, TValue2>(
            this IRxList<T> list, 
            Expression<Func<T, TValue1>> property1,
            Expression<Func<T, TValue2>> property2
        )
            where T : IRxObject
        {
            return list.ObserveElement(property1, property2, (x1, x2) => Unit.Default);
        }

        /// <summary>
        /// Observe the values of multiple properties. Each time the value of one of the properties changes, a new item
        /// will be emitted.  The value is based on resultSelector which has access to the current value of each property.
        /// </summary>
        public static IObservable<RxListObservedElement<T, TValue>> ObserveElement<T, TValue1, TValue2, TValue>(
            this IRxList<T> list, 
            Expression<Func<T, TValue1>> property1,
            Expression<Func<T, TValue2>> property2,
            Func<TValue1, TValue2, TValue> resultSelector
        )
            where T : IRxObject
        {
            var property1Observable = list.ObserveElementProperty(property1);
            var property2Observable = list.ObserveElementProperty(property2);
            return property1Observable.CombineLatest(property2Observable, (x, y) => new RxListObservedElement<T, TValue>(x.Element, resultSelector(x.Value, y.Value)));
        }

        /// <summary>
        /// Creates a list whose contents reflect the contents of the source list, but transformed in some way by the selector.  This is a 
        /// live list and will change accordingly when the source list is modified.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the derived list</typeparam>
        /// <typeparam name="T">The type of the elements in the source list</typeparam>
        /// <param name="source">The source list from which the derived list's contents will be derived</param>
        /// <param name="selector">The contents of the derived list are determined by applying this selector to each element in 
        /// the source list</param>
        /// <param name="removed">If not null, fired when an element from the source list is either removed or modified.  You 
        /// can take cleanup actions for the old items by using this callback.</param>
        /// <returns></returns>
        public static RxDerivedList<TSource, T> Derive<TSource, T>(this IRxList<TSource> source, Func<TSource, T> selector, Action<T> removed = null)
        {
            return new RxDerivedList<TSource, T>(source, selector, removed);
        }

        public static RxFilteredList<TSource, T, TValue> Derive<TSource, T, TValue>(this IRxList<TSource> source,
            Func<TSource, T> selector, Expression<Func<TSource, TValue>> filterSource,
            Func<TValue, bool> filter, Action<T> removed
        )
            where TSource : IRxObject
        {
            return new RxFilteredList<TSource, T, TValue>(source, selector, filterSource, filter, removed);
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IRxList<T> list)
        {
            var result = new ObservableCollection<T>(list);
            var disableRxEvents = false;
            var disableObservableCollectionEvents = false;
            list.Changed.SubscribeOnUiThread(x =>
            {
                if (disableRxEvents)
                    return;
                disableObservableCollectionEvents = true;
                foreach (var item in x.Added)
                {
                    result.Insert(item.Index, item.Value);
                }
                foreach (var item in x.Removed)
                {
                    result.Remove(item.Value);
                }
                foreach (var item in x.Modified)
                {
                    result[item.Index] = item.NewValue;
                }
                if (x.Moved != null)
                {
                    result.Move(x.Moved.Value.FromIndex, x.Moved.Value.ToIndex);
                }
                disableObservableCollectionEvents = false;
            });
            result.CollectionChanged += (sender, args) =>
            {
                if (disableObservableCollectionEvents)
                    return;
                disableRxEvents = true;
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        for (int i = 0, j = args.NewStartingIndex; i < args.NewItems.Count; i++, j++)
                        {
                            list.Insert(j, (T)args.NewItems[i]);
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var item in args.OldItems)
                        {
                            list.Remove((T)item);
                        }
                        break;
                    case NotifyCollectionChangedAction.Move:
                        list.Move(args.OldStartingIndex, args.NewStartingIndex);
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        list[args.NewStartingIndex] = (T)args.NewItems[0];
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        list.Clear();
                        list.AddRange(result);
                        break;
                }
                disableRxEvents = false;
            };
            return result;
        }

        /// <summary>
        /// Just a port of List.BinarySearch.  Searches the entire sorted collection for an element using the default comparer 
        /// and returns the zero-based index of the element.
        /// </summary>
        /// <returns>The zero-based index of item in the sorted List, if item is found; otherwise, a negative number that is the 
        /// bitwise complement of the index of the next element that is larger than item or, if there is no larger element, the bitwise 
        /// complement of Count.</returns>
        public static int BinarySearch<T, TOrderByValue>(this RxList<T> list, T value, Func<T, TOrderByValue> orderBy)
            where TOrderByValue : IComparable
        {
            const int index = 0;
            var length = list.Count;
            var lo = index;
            var hi = index + length - 1;
            while (lo <= hi)
            {
                var i = lo + ((hi - lo) >> 1);
                var order = orderBy(list[i]).CompareTo(orderBy(value));
 
                if (order == 0)
                {
                    return i;
                }

                if (order < 0)
                {
                    lo = i + 1;
                }
                else
                {
                    hi = i - 1;
                }
            }
 
            return ~lo;
        }

        /// <summary>
        /// Just a port of List.BinarySearch.  Searches the entire sorted collection for an element using the default comparer 
        /// and returns the zero-based index of the element.
        /// </summary>
        /// <returns>The zero-based index of item in the sorted List, if item is found; otherwise, a negative number that is the 
        /// bitwise complement of the index of the next element that is larger than item or, if there is no larger element, the bitwise 
        /// complement of Count.</returns>
        public static int BinarySearch<T, TValue>(this RxList<T> list, Func<T, TValue> selector, TValue value)
            where TValue : IComparable
        {
            const int index = 0;
            var length = list.Count;
            var lo = index;
            var hi = index + length - 1;
            while (lo <= hi)
            {
                var i = lo + ((hi - lo) >> 1);
                var order = selector(list[i]).CompareTo(value);
 
                if (order == 0)
                {
                    return i;
                }

                if (order < 0)
                {
                    lo = i + 1;
                }
                else
                {
                    hi = i - 1;
                }
            }
 
            return ~lo;
        }

        public static RxList<T> ToRxList<T>(this IEnumerable<T> source)
        {
            return new RxList<T>(source);
        }
    }
}