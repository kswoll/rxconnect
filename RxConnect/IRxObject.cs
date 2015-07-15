using System;
using System.Reactive;

namespace RxConnect
{
    public interface IRxObject : IDisposable
    {
        IObservable<IPropertyChanging> Changing { get; }
        IObservable<IPropertyChanged> Changed { get; }
    }
}