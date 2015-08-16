using System;
using System.Threading.Tasks;
using System.Reactive;

namespace SexyReact
{
    public interface IRxCommand : ICanExecute, IObservable<Unit>
    {
        Task ExecuteAsync();
    }

    public interface IRxCommand<in TInput> : ICanExecute, IObservable<Unit>
    {
        Task ExecuteAsync(TInput input);
    }
}

