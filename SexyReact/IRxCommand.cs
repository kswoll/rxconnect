using System;
using System.Threading.Tasks;
using System.Reactive;

namespace SexyReact
{
    public partial interface IRxCommand : ICanInvoke, IObservable<Unit>
    {
        Task InvokeAsync();
    }

    public partial interface IRxCommand<in TInput> : ICanInvoke, IObservable<Unit>
    {
        Task InvokeAsync(TInput input);
    }
}

