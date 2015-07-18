using System;
using System.Threading.Tasks;

namespace SexyReact
{
    public interface IRxFunction<TOutput> : IRxCommand, IObservable<TOutput>
    {
        new Task<TOutput> ExecuteAsync();
    }

    public interface IRxFunction<in TInput, TOutput> : IRxCommand<TInput>, IObservable<TOutput>
    {
        new Task<TOutput> ExecuteAsync(TInput input);
    }
}

