using System;
using System.Linq.Expressions;
using System.Reactive.Linq;

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
    }
}

