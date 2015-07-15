using System;
using System.Reflection;

namespace RxConnect
{
    public interface IObservePropertyStrategy : IDisposable
    {
        IObservable<TValue> ObservableForProperty<TValue>(PropertyInfo property);
        void OnNext<TValue>(PropertyInfo property, TValue value);
    }
}