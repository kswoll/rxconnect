using System;
using System.Linq;

namespace SexyReact
{
    public static class RxListExtensions
    {
//        public static IObservable<TValue> ObserveElement<T, TValue>(this RxList<T> list, Expression<Func<T, TValue>> selector)
//            where T : IRxObject
//        {
//            Func<IObservable<TValue>> merge = () => Observable.Merge(list.Select(x => x.ObserveProperty(selector)));
//            return list.Select(_ => merge()).ToObservable().Merge(list.Changed.Select(_ => merge())).Switch();
//        }

        public static RxDerivedList<T> Derive<TSource, T>(this IRxList<TSource> source, Func<TSource, T> selector)
        {
            var storage = new RxList<T>(source.Select(selector));
            var result = new RxDerivedList<T>(storage);

            var subscription = source.Changed.Subscribe(changes =>
            {
                if (changes.Added.Any())
                    storage.InsertRange(changes.Added.Select(x => new RxListItem<T>(x.Index, selector(x.Value))));
                if (changes.Removed.Any())
                    storage.RemoveRange(changes.Removed.Select(x => storage[x.Index]));
                if (changes.Modified.Any())
                    storage.ModifyRange(changes.Modified.Select(x => new RxListItem<T>(x.Index, selector(x.NewValue))));
                if (changes.Moved != null)
                    storage.Move(changes.Moved.Value.FromIndex, changes.Moved.Value.ToIndex);
            });

            // When the returned list is disposed, make sure to unsubscribe from the source list
            result.Disposed.Subscribe(_ => subscription.Dispose());

            return result;
        }
    }
}

