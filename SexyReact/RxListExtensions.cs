using System;
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
        public static RxDerivedList<T> Derive<TSource, T>(this IRxList<TSource> source, Func<TSource, T> selector, Action<T> removed = null)
        {
            var locker = new object();
            RxList<T> storage = null;

            Action<RxListChange<TSource>> onNext = changes =>
            {
                lock (locker)
                {
                    if (changes.Added.Any())
                    {
                        storage.InsertRange(changes.Added.Select(x => new RxListItem<T>(x.Index, selector(x.Value))));
                    }
                    foreach (var item in changes.Removed.OrderByDescending(x => x.Index))
                    {
                        removed?.Invoke(storage[item.Index]);
                        storage.RemoveAt(item.Index);
                    }
                    foreach (var item in changes.Modified)
                    {
                        removed?.Invoke(storage[item.Index]);
                        storage[item.Index] = selector(item.NewValue);
                    }
                    if (changes.Moved != null)
                    {
                        storage.Move(changes.Moved.Value.FromIndex, changes.Moved.Value.ToIndex);
                    }                    
                }
            };

            lock (locker)
            {
                storage = new RxList<T>(source.Select(selector));
                var result = new RxDerivedList<T>(storage);
                var subscription = source.Changed.Subscribe(onNext);

                // When the returned list is disposed, make sure to unsubscribe from the source list
                result.Disposed.Subscribe(_ => subscription.Dispose());

                return result;                
            }
        }
//
//        public static RxDerivedList<T> Derive<TSource, T>(this IRxList<TSource> source, Func<TSource, T> selector,
//            Func<TSource, IObservable<bool>> filter)
//        {
//            
//        }
    }
}