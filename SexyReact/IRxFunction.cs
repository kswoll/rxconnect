using System;
using System.Threading.Tasks;

namespace SexyReact
{
    public interface IRxFunction<TOutput> : IObservable<TOutput>
    {
        Task<TOutput> ExecuteAsync();
    }

    public interface IRxFunction<TInput, TOutput> : IObservable<TOutput>
    {
        Task<TOutput> ExecuteAsync(TInput input);
    }
}

