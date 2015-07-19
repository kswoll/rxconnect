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

        /// <summary>
        /// Produces an observable that returns the current value of the specified property as its value changes.  This handles
        /// property paths, so if you specify `x.Foo.Bar` and `x.Foo` is initially null, then the initial value of the observable
        /// is defualt(TValue).  When `Foo` becomes non-null the observable will emit the current value of `Bar`.  Finally, if 
        /// `Foo` becomes null, the observable again emits default(TValue).
        /// </summary>
        /// <returns>The property path that identifies the value of the property for the observable.</returns>
        /// <param name="obj">The IRxObject that contains the initial property in the path.</param>
        /// <param name="propertyPath">The sequence of properties that leads to the value.</param>
        /// <typeparam name="T">The type that contains the initial property in the path.</typeparam>
        /// <typeparam name="TValue">The value of the terminal property in the path.</typeparam>
        public static IObservable<TValue> ObserveProperty<T, TValue>(this T obj, params MemberInfo[] propertyPath)
            where T : IRxObject
        {
            var firstPropertyInfo = (PropertyInfo)propertyPath.First();
            foreach (var memberInfo in propertyPath.Take(propertyPath.Length - 1))
            {
                var propertyInfo = memberInfo as PropertyInfo;
                if (propertyInfo == null)
                    throw new ArgumentException("Member '" + string.Join(".", propertyPath.TakeWhile(x => x != propertyInfo)) + "' must be a property.", "propertyPath");
                if (!typeof(IRxObject).IsAssignableFrom(propertyInfo.PropertyType))
                    throw new ArgumentException("All properties leading up to the terminal property must represent an instance of IRxObject", "propertyPath");
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
            return lastObservable.DistinctUntilChanged();
        }

        /// <summary>
        /// Produces an observable that returns the current value of the specified property as its value changes.  This handles
        /// property paths, so if you specify `x.Foo.Bar` and `x.Foo` is initially null, then the initial value of the observable
        /// is defualt(TValue).  When `Foo` becomes non-null the observable will emit the current value of `Bar`.  Finally, if 
        /// `Foo` becomes null, the observable again emits default(TValue).
        /// </summary>
        /// <returns>The property path that identifies the value of the property for the observable.</returns>
        /// <param name="obj">The IRxObject that contains the initial property in the path.</param>
        /// <param name="propertyPath">The sequence of properties that leads to the value.</param>
        /// <typeparam name="T">The type that contains the initial property in the path.</typeparam>
        /// <typeparam name="TValue">The value of the terminal property in the path.</typeparam>
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
            if (memberExpression.Expression == property.Parameters[0])
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
        }

        /// <summary>
        /// Updates the value of the specified property based on the values that come down the observable.
        /// </summary>
        /// <param name="obj">The object whose property should be updated.</param>
        /// <param name="observable">The observable whose sequence of values will be used to update the value of the property.</param>
        /// <param name="property">The property whose value should be updated by the observabe.</param>
        /// <typeparam name="T">The type of the object that contains the property.</typeparam>
        /// <typeparam name="TValue">The property type of the properyt.</typeparam>
        public static void ObservableAsProperty<TValue>(this IRxObject obj, IObservable<TValue> observable, PropertyInfo property)
        {
            obj.Register(observable.DistinctUntilChanged().Subscribe(x => obj.Set(property, x)));
        }

        /// <summary>
        /// Updates the value of the specified property based on the values that come down the observable.
        /// </summary>
        /// <param name="obj">The object whose property should be updated.</param>
        /// <param name="observable">The observable whose sequence of values will be used to update the value of the property.</param>
        /// <param name="property">The property whose value should be updated by the observabe.</param>
        /// <typeparam name="T">The type of the object that contains the property.</typeparam>
        /// <typeparam name="TValue">The property type of the properyt.</typeparam>
        public static void ObservableAsProperty<T, TValue>(this T obj, IObservable<TValue> observable, Expression<Func<T, TValue>> property) 
            where T : IRxObject
        {
            var memberExpression = property.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("Lambda should specify a property.", "property");

            var propertyInfo = memberExpression.Member as PropertyInfo;
            if (propertyInfo == null)
                throw new ArgumentException("Member is not a property", "property");
            if (memberExpression.Expression != property.Parameters[0])
                throw new ArgumentException("Lambda should specify a property that exists directly on type " + typeof(T).FullName);

            obj.ObservableAsProperty(observable, propertyInfo);
        }
    }
}