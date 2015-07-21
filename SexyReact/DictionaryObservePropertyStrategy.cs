using System;
using System.Collections.Concurrent;
using System.Reactive.Subjects;
using System.Reflection;

namespace SexyReact
{
    public class DictionaryObservePropertyStrategy : IObservePropertyStrategy
    {
        private IRxObject obj;
        private ConcurrentDictionary<string, object> observables = new ConcurrentDictionary<string, object>();

        public DictionaryObservePropertyStrategy(IRxObject obj)
        {
            this.obj = obj;
        }

        private ReplaySubject<TValue> SubjectForProperty<TValue>(PropertyInfo property)
        {
            if (!property.DeclaringType.IsInstanceOfType(obj))
                throw new ArgumentException("Property '" + property.Name + "' is a member of " + property.DeclaringType.FullName + " but is being invoked against " + obj.GetType().FullName, "property");

            return (ReplaySubject<TValue>)observables.GetOrAdd(property.Name, x =>
            {
                var result = new ReplaySubject<TValue>(1);
                var currentValue = (TValue)property.GetValue(obj);
                result.OnNext(currentValue);
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