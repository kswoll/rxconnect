using System;
using System.Threading.Tasks;

namespace SexyReact
{
    public partial interface IRxFunction<TOutput> : IObservable<TOutput>, ICanInvoke
    {
        Task<TOutput> InvokeAsync();
    }

    public partial interface IRxFunction<in TInput, TOutput> : IObservable<TOutput>, ICanInvoke
    {
        Task<TOutput> InvokeAsync(TInput input);
    }
}

