using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Reflection;

namespace SexyReact
{
    public class DictionaryObservePropertyStrategy : IObservePropertyStrategy
    {
        private IRxObject obj;
        private Dictionary<string, object> observables = new Dictionary<string, object>();

        public DictionaryObservePropertyStrategy(IRxObject obj)
        {
            this.obj = obj;
        }

        private ReplaySubject<TValue> SubjectForProperty<TValue>(PropertyInfo property)
        {
            lock (observables)
            {
                object result;
                if (!observables.TryGetValue(property.Name, out result))
                {
                    var subject = new ReplaySubject<TValue>(1);
                    result = subject;
                    var currentValue = (TValue)property.GetValue(obj);
                    subject.OnNext(currentValue);
                    observables[property.Name] = result;
                }
                return (ReplaySubject<TValue>)result;
            }
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