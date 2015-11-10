using System;
using System.Reflection;

namespace SexyReact
{
    public class EmptyObservePropertyStrategy : IObservePropertyStrategy
    {
        public void Dispose()
        {
        }

        public IObservable<TValue> ObservableForProperty<TValue>(PropertyInfo property)
        {
            throw new NotImplementedException();
        }

        public void OnNext<TValue>(PropertyInfo property, TValue value)
        {
        }
    }
}
