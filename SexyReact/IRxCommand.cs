using System;
using System.Threading.Tasks;

namespace SexyReact
{
    public interface IRxCommand : ICanExecute
    {
        Task ExecuteAsync();
    }

    public interface IRxCommand<in TInput> : ICanExecute
    {
        Task ExecuteAsync(TInput input);
    }
}

