﻿using System;
using System.Reactive;
using System.Reflection;

namespace RxConnect
{
    public interface IRxObject : IDisposable
    {
        IObservable<IPropertyChanging> Changing { get; }
        IObservable<IPropertyChanged> Changed { get; }
        TValue Get<TValue>(PropertyInfo property);
        void Set<TValue>(PropertyInfo property, TValue value);
        IObservable<TValue> ObserveProperty<TValue>(PropertyInfo property);
    }
}