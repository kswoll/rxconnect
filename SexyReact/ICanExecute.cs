using System;

namespace SexyReact
{
    public interface ICanExecute
    {
        IObservable<bool> CanExecute { get; }
    }
}
