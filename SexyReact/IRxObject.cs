using System;
using System.Reflection;

namespace SexyReact
{
    public interface IRxObject : IDisposable
    {
        IObservable<IPropertyChanging> Changing { get; }
        IObservable<IPropertyChanged> Changed { get; }
        IObservable<IPropertyChanged> GetChangedByProperty(PropertyInfo property);
        IObservable<IPropertyChanging> GetChangingByProperty(PropertyInfo property);
        TValue Get<TValue>(PropertyInfo property);
        void Set<TValue>(PropertyInfo property, TValue value);
        IObservable<TValue> ObserveProperty<TValue>(PropertyInfo property);
        void Register(IDisposable disposable);
    }
}