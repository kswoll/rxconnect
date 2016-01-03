using System;
using System.Windows.Input;

namespace SexyReact
{
    public partial class RxCommand<TInput, TOutput>
    {
        private EventHandler canExecuteChanged;
        private bool canExecuteValue;

        partial void OnCreated()
        {
            CanInvoke.Subscribe(OnCanExecuteChanged);            
        }

        protected void OnCanExecuteChanged(bool canExecute)
        {
            canExecuteValue = canExecute;
            canExecuteChanged?.Invoke(this, new EventArgs());
        }

        bool ICommand.CanExecute(object parameter)
        {
            return canExecuteValue;
        }

        void ICommand.Execute(object parameter)
        {
            Invoke(parameter == null ? default(TInput) : (TInput)parameter);
        }

        event EventHandler ICommand.CanExecuteChanged
        {
            add { canExecuteChanged = (EventHandler)Delegate.Combine(canExecuteChanged, value); }
            remove { canExecuteChanged = (EventHandler)Delegate.Remove(canExecuteChanged, value); }
        }
    }
}