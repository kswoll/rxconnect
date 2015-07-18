using System;
using System.Threading.Tasks;

namespace SexyReact
{
    public interface IRxFunction<TOutput> : IObservable<TOutput>, ICanExecute
    {
        Task<TOutput> ExecuteAsync();
    }

    public interface IRxFunction<in TInput, TOutput> : IObservable<TOutput>, ICanExecute
    {
        Task<TOutput> ExecuteAsync(TInput input);
    }
}

