using System.Windows.Input;

namespace SexyReact
{
    public partial interface IRxCommand : ICommand
    {
    }

    public partial interface IRxCommand<in TInput> : ICommand
    {
    }
}