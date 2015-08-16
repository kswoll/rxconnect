using System;
using System.Linq;
using System.Linq.Expressions;

namespace SexyReact
{
    public static class RxListExtensions
    {
        public static IObservable<RxListObservedElement<T, TValue>> ObserveElement<T, TValue>(this IRxList<T> list, Expression<Func<T, TValue>> selector)
            where T : IRxObject
        {
            return new RxListElementObservable<T, TValue>(list, selector);
        }

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