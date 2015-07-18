using System;
using System.Threading.Tasks;

namespace SexyReact
{
        public interface IRxCommand
    {
        Task ExecuteAsync();
    }

    public interface IRxCommand<TInput>
    {
        Task ExecuteAsync(TInput input);
    }
}

