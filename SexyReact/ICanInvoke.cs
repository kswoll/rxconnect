using System;

namespace SexyReact
{
    public interface ICanInvoke
    {
        IObservable<bool> CanInvoke { get; }
    }
}
