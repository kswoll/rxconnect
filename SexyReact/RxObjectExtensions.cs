using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using SexyReact.Utils;

namespace SexyReact
{
    public static class RxObjectExtensions
    {
        private static readonly MethodInfo observePropertyMethod = typeof(IRxObject).GetMethod("ObserveProperty");
        private static readonly MethodInfo combineMethod = typeof(RxObjectExtensions).GetMethod("Combine", BindingFlags.Static | BindingFlags.NonPublic);

        public static IObservable<TValue> ObserveProperty<T, TValue>(this T obj, params MemberInfo[] propertyPath)
            where T : IRxObject
        {
            var firstPropertyInfo = (PropertyInfo)propertyPath.First();
            foreach (var memberInfo in propertyPath.Take(propertyPath.Length - 1))
            {
                var propertyInfo = memberInfo as PropertyInfo;
                if (propertyInfo == null)
                    throw new ArgumentException("Member '" + string.Join(".", propertyPath.TakeWhile(x => x != propertyInfo)) + "' must be a property.", "property");
                if (!typeof(IRxObject).IsAssignableFrom(propertyInfo.PropertyType))
                    throw new ArgumentException("All properties leading up to the terminal property must represent an instance of IRxObject", "property");
            }

            var firstObserveProperty = observePropertyMethod.MakeGenericMethod(firstPropertyInfo.PropertyType);
            var firstObservable = firstObserveProperty.Invoke(obj, new[] { firstPropertyInfo });
            var currentObservable = firstObservable;

            foreach (PropertyInfo propertyInfo in propertyPath.Skip(1))
            {
                var combine = combineMethod.MakeGenericMethod(propertyInfo.DeclaringType, propertyInfo.PropertyType);
                currentObservable = combine.Invoke(null, new[] { currentObservable, propertyInfo });
            }

            var lastObservable = (IObservable<TValue>)currentObservable;
            return lastObservable;
        }

        public static IObservable<TValue> ObserveProperty<T, TValue>(this T obj, Expression<Func<T, TValue>> property)
            where T : IRxObject
        {
            var memberExpression = property.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("Lambda should specify a property.", "property");

            var initialPropertyInfo = memberExpression.Member as PropertyInfo;
            if (initialPropertyInfo == null)
                throw new ArgumentException("Member is not a property", "property");

            // Handle the trivial case of x.Property efficiently
            if (memberExpression.Expression == null)
                return obj.ObserveProperty<TValue>(initialPropertyInfo);

            var propertyPath = property.GetPropertyPath().ToArray();
            return ObserveProperty<T, TValue>(obj, propertyPath);
        }

        private static IObservable<TValue> Combine<T, TValue>(IObservable<T> source, PropertyInfo property)
            where T : IRxObject
        {
            return source
                .Select(x => x == null ? Observable.Return(default(TValue)) : x.ObserveProperty<TValue>(property))
                .Switch();
                
//                new[]
//                {
//                    source.Where(x => x == null).Select(x => (default(TValue))),
//                    source.Where(x => x != null).SelectMany(x => x.ObserveProperty<TValue>(property))
//                }
//                .ToObservable()
//                .Switch();
//            return source.SelectMany(x => x == null ? Observable.Return(default(TValue)) : x.ObserveProperty<TValue>(property));
        }
    }
}