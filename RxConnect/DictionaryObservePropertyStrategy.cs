﻿using System;
using System.Collections.Concurrent;
using System.Reactive.Subjects;
using System.Reflection;

namespace RxConnect
{
    public class DictionaryObservePropertyStrategy : IObservePropertyStrategy
    {
        private IRxObject obj;
        private ConcurrentDictionary<PropertyInfo, object> observables = new ConcurrentDictionary<PropertyInfo, object>();

        public DictionaryObservePropertyStrategy(IRxObject obj)
        {
            this.obj = obj;
        }

        private ReplaySubject<TValue> SubjectForProperty<TValue>(PropertyInfo property)
        {
            if (!property.DeclaringType.IsInstanceOfType(obj))
                throw new ArgumentException("Property '" + property.Name + "' is a member of " + property.DeclaringType.FullName + " but is being invoked against " + obj.GetType().FullName, "property");

            return (ReplaySubject<TValue>)observables.GetOrAdd(property, x =>
            {
                var result = new ReplaySubject<TValue>(1);
                result.OnNext(obj.Get<TValue>(property));
                return result;
            });
        }

        public IObservable<TValue> ObservableForProperty<TValue>(PropertyInfo property)
        {
            return SubjectForProperty<TValue>(property);
        }

        public void OnNext<TValue>(PropertyInfo property, TValue value)
        {
            SubjectForProperty<TValue>(property).OnNext(value);
        }

        public void Dispose()
        {
            foreach (IDisposable value in observables.Values)
            {
                value.Dispose();
            }
        }
    }
}