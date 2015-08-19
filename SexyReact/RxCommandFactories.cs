using System;
using System.Reactive;
using System.Threading.Tasks;

namespace SexyReact
{
    public static class RxCommand
    {
        /// <summary>
        /// Creates a command that consumes no input and produces no output.  Non async version.
        /// </summary>
        /// <param name="action">The action to execute when invoking the command.</param>
        /// <param name="canExecute">An observable that dictates whether or not the command may execute. If not 
        /// specified, an observable is created that produces true.</param>
        /// <param name="allowSimultaneousExecution">If true, multiple execution of this command may be performed.  If false, 
        /// then subsequent calls to ExecuteAsync return the defaultValue until the execution of the initial invocation completes.</param>
        public static IRxCommand Create(Action action, IObservable<bool> canExecute = null, bool allowSimultaneousExecution = false)
        {
            return CreateAsync(
                () => 
                {
                    action();
                    return Task.FromResult(default(Unit));
                }, 
                canExecute, 
                allowSimultaneousExecution: allowSimultaneousExecution);
        }

        /// <summary>
        /// Creates a command that consumes no input and produces no output.
        /// </summary>
        /// <param name="action">The action to execute when invoking the command.</param>
        /// <param name="canExecute">An observable that dictates whether or not the command may execute. If not 
        /// specified, an observable is created that produces true.</param>
        /// <param name="allowSimultaneousExecution">If true, multiple execution of this command may be performed.  If false, 
        /// then subsequent calls to ExecuteAsync return the defaultValue until the execution of the initial invocation completes.</param>
        public static IRxCommand CreateAsync(Func<Task> action, IObservable<bool> canExecute = null, bool allowSimultaneousExecution = false)
        {
            return new RxCommand<Unit>(
                async x => 
                {
                    await action();
                    return default(Unit);
                }, 
                canExecute,
                allowSimultaneousExecution: allowSimultaneousExecution);
        }

        /// <summary>
        /// Creates a command that consumes input, but produces no output.  Non async version.
        /// </summary>
        /// <param name="action">The action to execute when invoking the command.</param>
        /// <param name="canExecute">An observable that dictates whether or not the command may execute. If not 
        /// specified, an observable is created that produces true.</param>
        /// <param name="allowSimultaneousExecution">If true, multiple execution of this command may be performed.  If false, 
        /// then subsequent calls to ExecuteAsync return the defaultValue until the execution of the initial invocation completes.</param>
        public static IRxCommand<TInput> Create<TInput>(Action<TInput> action, IObservable<bool> canExecute = null, bool allowSimultaneousExecution = false)
        {
            return new RxCommand<TInput>(
                x =>
                {
                    action(x);
                    return Task.FromResult(default(Unit));
                }, 
                canExecute,
                allowSimultaneousExecution: allowSimultaneousExecution);
        }

        /// <summary>
        /// Creates a command that consumes input, but produces no output.
        /// </summary>
        /// <param name="action">The action to execute when invoking the command.</param>
        /// <param name="canExecute">An observable that dictates whether or not the command may execute. If not 
        /// specified, an observable is created that produces true.</param>
        /// <param name="allowSimultaneousExecution">If true, multiple execution of this command may be performed.  If false, 
        /// then subsequent calls to ExecuteAsync return the defaultValue until the execution of the initial invocation completes.</param>
        public static IRxCommand<TInput> CreateAsync<TInput>(Func<TInput, Task> action, IObservable<bool> canExecute = null, bool allowSimultaneousExecution = false)
        {
            return new RxCommand<TInput>(
                async x =>
                {
                    await action(x);
                    return default(Unit);
                }, 
                canExecute,
                allowSimultaneousExecution: allowSimultaneousExecution);
        }
    }
}
