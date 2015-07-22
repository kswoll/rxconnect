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
#if MONOTOUCH
        private static readonly MethodInfo observePropertyAsObjectMethod = typeof(RxObjectExtensions).GetMethod("ObservePropertyAsObject");
#else
        private static readonly MethodInfo combineMethod = typeof(RxObjectExtensions).GetMethod("Combine", BindingFlags.Static | BindingFlags.NonPublic);
#endif
        public static IObservable<object> ObservePropertyAsObject<TValue>(this IRxObject obj, PropertyInfo property)
        {
            return obj.ObserveProperty<TValue>(property).Select(x => (object)x);
        }

        public static IObservable<TValue> ObserveProperty<T, TValue>(this IObservable<T> observable, params MemberInfo[] propertyPath)
        {
            return ObserveProperty<TValue>(observable, propertyPath);
        }

        private static IObservable<TValue> ObserveProperty<TValue>(object observable, params MemberInfo[] propertyPath)
        {
            object currentObservable = observable;

            for (var i = 0; i < propertyPath.Length; i++)
            {
                var memberInfo = propertyPath[i];
                var propertyInfo = memberInfo as PropertyInfo;
                if (propertyInfo == null)
                    throw new ArgumentException("Member '" + string.Join(".", propertyPath.TakeWhile(x => x != propertyInfo)) + "' must be a property.", "propertyPath");
                if (i < propertyPath.Length - 1 && !typeof(IRxObject).IsAssignableFrom(propertyInfo.PropertyType))
                    throw new ArgumentException("All properties leading up to the terminal property must represent an instance of IRxObject", "propertyPath");

                #if MONOTOUCH
                currentObservable = Combine((IObservable<object>)currentObservable, propertyInfo, GetDefaultValue(propertyInfo.PropertyType));
                #else
                var combine = combineMethod.MakeGenericMethod(propertyInfo.DeclaringType, propertyInfo.PropertyType);
                currentObservable = combine.Invoke(null, new[] { currentObservable, propertyInfo });
                #endif
            }

            #if MONOTOUCH
            var lastObservable = ((IObservable<object>)currentObservable).Cast<TValue>();
            #else
            var lastObservable = (IObservable<TValue>)currentObservable;
            #endif

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
        public static IObservable<TValue> ObserveProperty<T, TValue>(this T obj, params MemberInfo[] propertyPath)
            where T : IRxObject
        {
            var firstPropertyInfo = (PropertyInfo)propertyPath[0];
            var firstObserveProperty = observePropertyMethod.MakeGenericMethod(firstPropertyInfo.PropertyType);
            var firstObservable = firstObserveProperty.Invoke(obj, new[] { firstPropertyInfo });
            return ObserveProperty<TValue>(firstObservable, propertyPath.Skip(1).ToArray());
        }

#if MONOTOUCH
        private static IObservable<object> Combine(IObservable<object> source, PropertyInfo property, object defaultValue)
        {
            return source
                .Select(x => x == null ? Observable.Return(defaultValue) : (IObservable<object>)observePropertyAsObjectMethod.MakeGenericMethod(property.PropertyType).Invoke(null, new object[] { x, property }))
                .Switch();
        }

        private static object GetDefaultValue(Type type)
        {
            return typeof(DefaultValue<>).MakeGenericType(type).GetField("Value").GetValue(null);
        }

        private static class DefaultValue<T>
        {
            public static readonly T Value = default(T);
        }
#else
        private static IObservable<TValue> Combine<T, TValue>(IObservable<T> source, PropertyInfo property)
            where T : IRxObject
        {
            return source
                .Select(x => x == null ? Observable.Return(default(TValue)) : x.ObserveProperty<TValue>(property))
                .Switch();
        }
#endif

        /// <summary>
        /// Produces an observable that returns the current value of the specified property as its value changes.  This handles
        /// property paths, so if you specify `x.Foo.Bar` and `x.Foo` is initially null, then the initial value of the observable
        /// is defualt(TValue).  When `Foo` becomes non-null the observable will emit the current value of `Bar`.  Finally, if 
        /// `Foo` becomes null, the observable again emits default(TValue).
        /// </summary>
        /// <returns>The property path that identifies the value of the property for the observable.</returns>
        /// <param name="obj">The IRxObject that contains the initial property in the path.</param>
        /// <param name="property">The property whose value should be observed.</param>
        /// <typeparam name="T">The type that contains the initial property in the path.</typeparam>
        /// <typeparam name="TValue">The value of the terminal property in the path.</typeparam>
        public static IObservable<TValue> ObserveProperty<T, TValue>(this T obj, Expression<Func<T, TValue>> property)
            where T : IRxObject
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
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

        /// <summary>
        /// Observe the values of multiple properties. Each time the value of one of the properties changes, a new item
        /// will be emitted.  The value is based on resultSelector which has access to the current value of each property.
        /// </summary>
        public static IObservable<TValue> ObserveProperties<T, TValue1, TValue2, TValue>(
            this T obj, 
            Expression<Func<T, TValue1>> property1,
            Expression<Func<T, TValue2>> property2,
            Func<TValue1, TValue2, TValue> resultSelector
        )
            where T : IRxObject
        {
            var property1Observable = obj.ObserveProperty(property1);
            var property2Observable = obj.ObserveProperty(property2);
            return property1Observable.CombineLatest(property2Observable, resultSelector);
        }

        /// <summary>
        /// Observe the values of multiple properties. Each time the value of one of the properties changes, a new item
        /// will be emitted.  The value is based on resultSelector which has access to the current value of each property.
        /// </summary>
        public static IObservable<TValue> ObserveProperties<T, TValue1, TValue2, TValue3, TValue>(
            this T obj, 
            Expression<Func<T, TValue1>> property1,
            Expression<Func<T, TValue2>> property2,
            Expression<Func<T, TValue3>> property3,
            Func<TValue1, TValue2, TValue3, TValue> resultSelector
        )
            where T : IRxObject
        {
            var property1Observable = obj.ObserveProperty(property1);
            var property2Observable = obj.ObserveProperty(property2);
            var property3Observable = obj.ObserveProperty(property3);
            return property1Observable.CombineLatest(property2Observable, property3Observable, resultSelector);
        }

        /// <summary>
        /// Observe the values of multiple properties. Each time the value of one of the properties changes, a new item
        /// will be emitted.  The value is based on resultSelector which has access to the current value of each property.
        /// </summary>
        public static IObservable<TValue> ObserveProperties<T, TValue1, TValue2, TValue3, TValue4, TValue>(
            this T obj, 
            Expression<Func<T, TValue1>> property1,
            Expression<Func<T, TValue2>> property2,
            Expression<Func<T, TValue3>> property3,
            Expression<Func<T, TValue4>> property4,
            Func<TValue1, TValue2, TValue3, TValue4, TValue> resultSelector
        )
            where T : IRxObject
        {
            var property1Observable = obj.ObserveProperty(property1);
            var property2Observable = obj.ObserveProperty(property2);
            var property3Observable = obj.ObserveProperty(property3);
            var property4Observable = obj.ObserveProperty(property4);
            return property1Observable.CombineLatest(property2Observable, property3Observable, property4Observable, resultSelector);
        }

        /// <summary>
        /// Observe the values of multiple properties. Each time the value of one of the properties changes, a new item
        /// will be emitted.  The value is based on resultSelector which has access to the current value of each property.
        /// </summary>
        public static IObservable<TValue> ObserveProperties<T, TValue1, TValue2, TValue3, TValue4, TValue5, TValue>(
            this T obj, 
            Expression<Func<T, TValue1>> property1,
            Expression<Func<T, TValue2>> property2,
            Expression<Func<T, TValue3>> property3,
            Expression<Func<T, TValue4>> property4,
            Expression<Func<T, TValue5>> property5,
            Func<TValue1, TValue2, TValue3, TValue4, TValue5, TValue> resultSelector
        )
            where T : IRxObject
        {
            var property1Observable = obj.ObserveProperty(property1);
            var property2Observable = obj.ObserveProperty(property2);
            var property3Observable = obj.ObserveProperty(property3);
            var property4Observable = obj.ObserveProperty(property4);
            var property5Observable = obj.ObserveProperty(property5);
            return property1Observable.CombineLatest(property2Observable, property3Observable, property4Observable, property5Observable, 
                resultSelector);
        }

        /// <summary>
        /// Observe the values of multiple properties. Each time the value of one of the properties changes, a new item
        /// will be emitted.  The value is based on resultSelector which has access to the current value of each property.
        /// </summary>
        public static IObservable<TValue> ObserveProperties<T, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue>(
            this T obj, 
            Expression<Func<T, TValue1>> property1,
            Expression<Func<T, TValue2>> property2,
            Expression<Func<T, TValue3>> property3,
            Expression<Func<T, TValue4>> property4,
            Expression<Func<T, TValue5>> property5,
            Expression<Func<T, TValue6>> property6,
            Func<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue> resultSelector
        )
            where T : IRxObject
        {
            var property1Observable = obj.ObserveProperty(property1);
            var property2Observable = obj.ObserveProperty(property2);
            var property3Observable = obj.ObserveProperty(property3);
            var property4Observable = obj.ObserveProperty(property4);
            var property5Observable = obj.ObserveProperty(property5);
            var property6Observable = obj.ObserveProperty(property6);
            return property1Observable.CombineLatest(property2Observable, property3Observable, property4Observable, property5Observable, 
                property6Observable, resultSelector);
        }

        /// <summary>
        /// Observe the values of multiple properties. Each time the value of one of the properties changes, a new item
        /// will be emitted.  The value is based on resultSelector which has access to the current value of each property.
        /// </summary>
        public static IObservable<TValue> ObserveProperties<T, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue>(
            this T obj, 
            Expression<Func<T, TValue1>> property1,
            Expression<Func<T, TValue2>> property2,
            Expression<Func<T, TValue3>> property3,
            Expression<Func<T, TValue4>> property4,
            Expression<Func<T, TValue5>> property5,
            Expression<Func<T, TValue6>> property6,
            Expression<Func<T, TValue7>> property7,
            Func<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue> resultSelector
        )
            where T : IRxObject
        {
            var property1Observable = obj.ObserveProperty(property1);
            var property2Observable = obj.ObserveProperty(property2);
            var property3Observable = obj.ObserveProperty(property3);
            var property4Observable = obj.ObserveProperty(property4);
            var property5Observable = obj.ObserveProperty(property5);
            var property6Observable = obj.ObserveProperty(property6);
            var property7Observable = obj.ObserveProperty(property7);
            return property1Observable.CombineLatest(property2Observable, property3Observable, property4Observable, property5Observable, 
                property6Observable, property7Observable, resultSelector);
        }

        /// <summary>
        /// Observe the values of multiple properties. Each time the value of one of the properties changes, a new item
        /// will be emitted.  The value is based on resultSelector which has access to the current value of each property.
        /// </summary>
        public static IObservable<TValue> ObserveProperties<T, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8, TValue>(
            this T obj, 
            Expression<Func<T, TValue1>> property1,
            Expression<Func<T, TValue2>> property2,
            Expression<Func<T, TValue3>> property3,
            Expression<Func<T, TValue4>> property4,
            Expression<Func<T, TValue5>> property5,
            Expression<Func<T, TValue6>> property6,
            Expression<Func<T, TValue7>> property7,
            Expression<Func<T, TValue8>> property8,
            Func<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8, TValue> resultSelector
        )
            where T : IRxObject
        {
            var property1Observable = obj.ObserveProperty(property1);
            var property2Observable = obj.ObserveProperty(property2);
            var property3Observable = obj.ObserveProperty(property3);
            var property4Observable = obj.ObserveProperty(property4);
            var property5Observable = obj.ObserveProperty(property5);
            var property6Observable = obj.ObserveProperty(property6);
            var property7Observable = obj.ObserveProperty(property7);
            var property8Observable = obj.ObserveProperty(property8);
            return property1Observable.CombineLatest(property2Observable, property3Observable, property4Observable, property5Observable, 
                property6Observable, property7Observable, property8Observable, resultSelector);
        }

        /// <summary>
        /// Observe the values of multiple properties. Each time the value of one of the properties changes, a new item
        /// will be emitted.  The value is based on resultSelector which has access to the current value of each property.
        /// </summary>
        public static IObservable<TValue> ObserveProperties<T, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8, TValue9, TValue>(
            this T obj, 
            Expression<Func<T, TValue1>> property1,
            Expression<Func<T, TValue2>> property2,
            Expression<Func<T, TValue3>> property3,
            Expression<Func<T, TValue4>> property4,
            Expression<Func<T, TValue5>> property5,
            Expression<Func<T, TValue6>> property6,
            Expression<Func<T, TValue7>> property7,
            Expression<Func<T, TValue8>> property8,
            Expression<Func<T, TValue9>> property9,
            Func<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8, TValue9, TValue> resultSelector
        )
            where T : IRxObject
        {
            var property1Observable = obj.ObserveProperty(property1);
            var property2Observable = obj.ObserveProperty(property2);
            var property3Observable = obj.ObserveProperty(property3);
            var property4Observable = obj.ObserveProperty(property4);
            var property5Observable = obj.ObserveProperty(property5);
            var property6Observable = obj.ObserveProperty(property6);
            var property7Observable = obj.ObserveProperty(property7);
            var property8Observable = obj.ObserveProperty(property8);
            var property9Observable = obj.ObserveProperty(property9);
            return property1Observable.CombineLatest(property2Observable, property3Observable, property4Observable, property5Observable, 
                property6Observable, property7Observable, property8Observable, property9Observable, resultSelector);
        }

        /// <summary>
        /// Observe the values of multiple properties. Each time the value of one of the properties changes, a new item
        /// will be emitted.  The value is based on resultSelector which has access to the current value of each property.
        /// </summary>
        public static IObservable<TValue> ObserveProperties<T, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8, 
            TValue9, TValue10, TValue>
        (
            this T obj, 
            Expression<Func<T, TValue1>> property1,
            Expression<Func<T, TValue2>> property2,
            Expression<Func<T, TValue3>> property3,
            Expression<Func<T, TValue4>> property4,
            Expression<Func<T, TValue5>> property5,
            Expression<Func<T, TValue6>> property6,
            Expression<Func<T, TValue7>> property7,
            Expression<Func<T, TValue8>> property8,
            Expression<Func<T, TValue9>> property9,
            Expression<Func<T, TValue10>> property10,
            Func<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8, TValue9, TValue10, TValue> resultSelector
        )
            where T : IRxObject
        {
            var property1Observable = obj.ObserveProperty(property1);
            var property2Observable = obj.ObserveProperty(property2);
            var property3Observable = obj.ObserveProperty(property3);
            var property4Observable = obj.ObserveProperty(property4);
            var property5Observable = obj.ObserveProperty(property5);
            var property6Observable = obj.ObserveProperty(property6);
            var property7Observable = obj.ObserveProperty(property7);
            var property8Observable = obj.ObserveProperty(property8);
            var property9Observable = obj.ObserveProperty(property9);
            var property10Observable = obj.ObserveProperty(property10);
            return property1Observable.CombineLatest(property2Observable, property3Observable, property4Observable, property5Observable, 
                property6Observable, property7Observable, property8Observable, property9Observable, property10Observable, resultSelector);
        }

        /// <summary>
        /// Observe the values of multiple properties. Each time the value of one of the properties changes, a new item
        /// will be emitted.  The value is based on resultSelector which has access to the current value of each property.
        /// </summary>
        public static IObservable<TValue> ObserveProperties<T, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8, 
            TValue9, TValue10, TValue11, TValue>
        (
            this T obj, 
            Expression<Func<T, TValue1>> property1,
            Expression<Func<T, TValue2>> property2,
            Expression<Func<T, TValue3>> property3,
            Expression<Func<T, TValue4>> property4,
            Expression<Func<T, TValue5>> property5,
            Expression<Func<T, TValue6>> property6,
            Expression<Func<T, TValue7>> property7,
            Expression<Func<T, TValue8>> property8,
            Expression<Func<T, TValue9>> property9,
            Expression<Func<T, TValue10>> property10,
            Expression<Func<T, TValue11>> property11,
            Func<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8, TValue9, TValue10, TValue11, TValue> resultSelector
        )
            where T : IRxObject
        {
            var property1Observable = obj.ObserveProperty(property1);
            var property2Observable = obj.ObserveProperty(property2);
            var property3Observable = obj.ObserveProperty(property3);
            var property4Observable = obj.ObserveProperty(property4);
            var property5Observable = obj.ObserveProperty(property5);
            var property6Observable = obj.ObserveProperty(property6);
            var property7Observable = obj.ObserveProperty(property7);
            var property8Observable = obj.ObserveProperty(property8);
            var property9Observable = obj.ObserveProperty(property9);
            var property10Observable = obj.ObserveProperty(property10);
            var property11Observable = obj.ObserveProperty(property11);
            return property1Observable.CombineLatest(property2Observable, property3Observable, property4Observable, property5Observable, 
                property6Observable, property7Observable, property8Observable, property9Observable, property10Observable, 
                property11Observable, resultSelector);
        }

        /// <summary>
        /// Observe the values of multiple properties. Each time the value of one of the properties changes, a new item
        /// will be emitted.  The value is based on resultSelector which has access to the current value of each property.
        /// </summary>
        public static IObservable<TValue> ObserveProperties<T, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8, 
            TValue9, TValue10, TValue11, TValue12, TValue>
        (
            this T obj, 
            Expression<Func<T, TValue1>> property1,
            Expression<Func<T, TValue2>> property2,
            Expression<Func<T, TValue3>> property3,
            Expression<Func<T, TValue4>> property4,
            Expression<Func<T, TValue5>> property5,
            Expression<Func<T, TValue6>> property6,
            Expression<Func<T, TValue7>> property7,
            Expression<Func<T, TValue8>> property8,
            Expression<Func<T, TValue9>> property9,
            Expression<Func<T, TValue10>> property10,
            Expression<Func<T, TValue11>> property11,
            Expression<Func<T, TValue12>> property12,
            Func<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8, TValue9, TValue10, TValue11, TValue12, TValue> resultSelector
        )
            where T : IRxObject
        {
            var property1Observable = obj.ObserveProperty(property1);
            var property2Observable = obj.ObserveProperty(property2);
            var property3Observable = obj.ObserveProperty(property3);
            var property4Observable = obj.ObserveProperty(property4);
            var property5Observable = obj.ObserveProperty(property5);
            var property6Observable = obj.ObserveProperty(property6);
            var property7Observable = obj.ObserveProperty(property7);
            var property8Observable = obj.ObserveProperty(property8);
            var property9Observable = obj.ObserveProperty(property9);
            var property10Observable = obj.ObserveProperty(property10);
            var property11Observable = obj.ObserveProperty(property11);
            var property12Observable = obj.ObserveProperty(property12);
            return property1Observable.CombineLatest(property2Observable, property3Observable, property4Observable, property5Observable, 
                property6Observable, property7Observable, property8Observable, property9Observable, property10Observable, 
                property11Observable, property12Observable, resultSelector);
        }

        /// <summary>
        /// Observe the values of multiple properties. Each time the value of one of the properties changes, a new item
        /// will be emitted.  The value is based on resultSelector which has access to the current value of each property.
        /// </summary>
        public static IObservable<TValue> ObserveProperties<T, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8, 
            TValue9, TValue10, TValue11, TValue12, TValue13, TValue>
        (
            this T obj, 
            Expression<Func<T, TValue1>> property1,
            Expression<Func<T, TValue2>> property2,
            Expression<Func<T, TValue3>> property3,
            Expression<Func<T, TValue4>> property4,
            Expression<Func<T, TValue5>> property5,
            Expression<Func<T, TValue6>> property6,
            Expression<Func<T, TValue7>> property7,
            Expression<Func<T, TValue8>> property8,
            Expression<Func<T, TValue9>> property9,
            Expression<Func<T, TValue10>> property10,
            Expression<Func<T, TValue11>> property11,
            Expression<Func<T, TValue12>> property12,
            Expression<Func<T, TValue13>> property13,
            Func<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8, TValue9, TValue10, TValue11, TValue12, TValue13, TValue> resultSelector
        )
            where T : IRxObject
        {
            var property1Observable = obj.ObserveProperty(property1);
            var property2Observable = obj.ObserveProperty(property2);
            var property3Observable = obj.ObserveProperty(property3);
            var property4Observable = obj.ObserveProperty(property4);
            var property5Observable = obj.ObserveProperty(property5);
            var property6Observable = obj.ObserveProperty(property6);
            var property7Observable = obj.ObserveProperty(property7);
            var property8Observable = obj.ObserveProperty(property8);
            var property9Observable = obj.ObserveProperty(property9);
            var property10Observable = obj.ObserveProperty(property10);
            var property11Observable = obj.ObserveProperty(property11);
            var property12Observable = obj.ObserveProperty(property12);
            var property13Observable = obj.ObserveProperty(property13);
            return property1Observable.CombineLatest(property2Observable, property3Observable, property4Observable, property5Observable, 
                property6Observable, property7Observable, property8Observable, property9Observable, property10Observable, 
                property11Observable, property12Observable, property13Observable, resultSelector);
        }

        /// <summary>
        /// Observe the values of multiple properties. Each time the value of one of the properties changes, a new item
        /// will be emitted.  The value is based on resultSelector which has access to the current value of each property.
        /// </summary>
        public static IObservable<TValue> ObserveProperties<T, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8, 
            TValue9, TValue10, TValue11, TValue12, TValue13, TValue14, TValue>
        (
            this T obj, 
            Expression<Func<T, TValue1>> property1,
            Expression<Func<T, TValue2>> property2,
            Expression<Func<T, TValue3>> property3,
            Expression<Func<T, TValue4>> property4,
            Expression<Func<T, TValue5>> property5,
            Expression<Func<T, TValue6>> property6,
            Expression<Func<T, TValue7>> property7,
            Expression<Func<T, TValue8>> property8,
            Expression<Func<T, TValue9>> property9,
            Expression<Func<T, TValue10>> property10,
            Expression<Func<T, TValue11>> property11,
            Expression<Func<T, TValue12>> property12,
            Expression<Func<T, TValue13>> property13,
            Expression<Func<T, TValue14>> property14,
            Func<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8, TValue9, TValue10, TValue11, TValue12, TValue13, TValue14, TValue> resultSelector
        )
            where T : IRxObject
        {
            var property1Observable = obj.ObserveProperty(property1);
            var property2Observable = obj.ObserveProperty(property2);
            var property3Observable = obj.ObserveProperty(property3);
            var property4Observable = obj.ObserveProperty(property4);
            var property5Observable = obj.ObserveProperty(property5);
            var property6Observable = obj.ObserveProperty(property6);
            var property7Observable = obj.ObserveProperty(property7);
            var property8Observable = obj.ObserveProperty(property8);
            var property9Observable = obj.ObserveProperty(property9);
            var property10Observable = obj.ObserveProperty(property10);
            var property11Observable = obj.ObserveProperty(property11);
            var property12Observable = obj.ObserveProperty(property12);
            var property13Observable = obj.ObserveProperty(property13);
            var property14Observable = obj.ObserveProperty(property14);
            return property1Observable.CombineLatest(property2Observable, property3Observable, property4Observable, property5Observable, 
                property6Observable, property7Observable, property8Observable, property9Observable, property10Observable, 
                property11Observable, property12Observable, property13Observable, property14Observable, resultSelector);
        }

        /// <summary>
        /// Observe the values of multiple properties. Each time the value of one of the properties changes, a new item
        /// will be emitted.  The value is based on resultSelector which has access to the current value of each property.
        /// </summary>
        public static IObservable<TValue> ObserveProperties<T, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8, 
            TValue9, TValue10, TValue11, TValue12, TValue13, TValue14, TValue15, TValue>
        (
            this T obj, 
            Expression<Func<T, TValue1>> property1,
            Expression<Func<T, TValue2>> property2,
            Expression<Func<T, TValue3>> property3,
            Expression<Func<T, TValue4>> property4,
            Expression<Func<T, TValue5>> property5,
            Expression<Func<T, TValue6>> property6,
            Expression<Func<T, TValue7>> property7,
            Expression<Func<T, TValue8>> property8,
            Expression<Func<T, TValue9>> property9,
            Expression<Func<T, TValue10>> property10,
            Expression<Func<T, TValue11>> property11,
            Expression<Func<T, TValue12>> property12,
            Expression<Func<T, TValue13>> property13,
            Expression<Func<T, TValue14>> property14,
            Expression<Func<T, TValue15>> property15,
            Func<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8, TValue9, TValue10, TValue11, TValue12, TValue13, TValue14, TValue15, TValue> resultSelector
        )
            where T : IRxObject
        {
            var property1Observable = obj.ObserveProperty(property1);
            var property2Observable = obj.ObserveProperty(property2);
            var property3Observable = obj.ObserveProperty(property3);
            var property4Observable = obj.ObserveProperty(property4);
            var property5Observable = obj.ObserveProperty(property5);
            var property6Observable = obj.ObserveProperty(property6);
            var property7Observable = obj.ObserveProperty(property7);
            var property8Observable = obj.ObserveProperty(property8);
            var property9Observable = obj.ObserveProperty(property9);
            var property10Observable = obj.ObserveProperty(property10);
            var property11Observable = obj.ObserveProperty(property11);
            var property12Observable = obj.ObserveProperty(property12);
            var property13Observable = obj.ObserveProperty(property13);
            var property14Observable = obj.ObserveProperty(property14);
            var property15Observable = obj.ObserveProperty(property15);
            return property1Observable.CombineLatest(property2Observable, property3Observable, property4Observable, property5Observable, 
                property6Observable, property7Observable, property8Observable, property9Observable, property10Observable, 
                property11Observable, property12Observable, property13Observable, property14Observable, property15Observable, resultSelector);
        }

        /// <summary>
        /// Updates the value of the specified property based on the values that come down the observable.
        /// </summary>
        /// <param name="obj">The object whose property should be updated.</param>
        /// <param name="observable">The observable whose sequence of values will be used to update the value of the property.</param>
        /// <param name="property">The property whose value should be updated by the observabe.</param>
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