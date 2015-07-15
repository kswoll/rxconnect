using System;
using System.Linq.Expressions;
using RxConnect.Utils;

namespace RxConnect
{
    public static class RxObjectExtensions
    {
        public static IObservable<TValue> ObserveProperty<T, TValue>(this T obj, Expression<Func<T, TValue>> property)
            where T : IRxObject
        {
            var propertyInfo = property.GetPropertyInfo();
            return obj.ObserveProperty<TValue>(propertyInfo);
        }
    }
}