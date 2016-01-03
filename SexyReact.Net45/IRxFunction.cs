using System.Windows.Input;

namespace SexyReact
{
    public partial interface IRxFunction<TOutput> : ICommand
    {
    }

    public partial interface IRxFunction<in TInput, TOutput> : ICommand
    {
    }
}