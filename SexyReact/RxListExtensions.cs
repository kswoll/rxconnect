using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

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
        public static IObservable<RxListObservedElement<T, TValue>> ObserveElement<T, TValue>(this IRxList<T> list, Expression<Func<T, TValue>> selector)
            where T : IRxObject
        {
            return new RxListElementObservable<T, TValue>(list, selector);
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
            list.Changed.Subscribe(x =>
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
    }
}