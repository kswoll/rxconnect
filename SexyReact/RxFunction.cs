using System;
using System.Reactive;
using System.Threading.Tasks;

namespace SexyReact
{
    public static class RxFunction
    {
        /// <summary>
        /// Creates a command that consumes no input, but produces output.  Non async version.
        /// </summary>
        /// <param name="action">The action to execute when invoking the command.</param>
        /// <param name="canExecute">An observable that dictates whether or not the command may execute. If not 
        /// specified, an observable is created that produces true.</param>
        /// <param name="defaultValue">A factory function to provide the return value for when the method fails to execute.</param>
        /// <param name="allowSimultaneousExecution">If true, multiple execution of this command may be performed.  If false, 
        /// then subsequent calls to ExecuteAsync return the defaultValue until the execution of the initial invocation completes.</param>
        public static IRxFunction<TOutput> Create<TOutput>(Func<TOutput> action, IObservable<bool> canExecute = null, Func<TOutput> defaultValue = null, bool allowSimultaneousExecution = false)
        {
            return new RxCommand<Unit, TOutput>(x => Task.FromResult(action()), canExecute, defaultValue, allowSimultaneousExecution);
        }

        /// <summary>
        /// Creates a command that consumes no input, but produces output.
        /// </summary>
        /// <param name="action">The action to execute when invoking the command.</param>
        /// <param name="canExecute">An observable that dictates whether or not the command may execute. If not 
        /// specified, an observable is created that produces true.</param>
        /// <param name="defaultValue">A factory function to provide the return value for when the method fails to execute.</param>
        /// <param name="allowSimultaneousExecution">If true, multiple execution of this command may be performed.  If false, 
        /// then subsequent calls to ExecuteAsync return the defaultValue until the execution of the initial invocation completes.</param>
        public static IRxFunction<TOutput> CreateAsync<TOutput>(Func<Task<TOutput>> action, IObservable<bool> canExecute = null, Func<TOutput> defaultValue = null, bool allowSimultaneousExecution = false)
        {
            return new RxCommand<Unit, TOutput>(x => action(), canExecute, defaultValue, allowSimultaneousExecution);
        }

        /// <summary>
        /// Creates a command that consumes input and produces output.  Non async version.
        /// </summary>
        /// <param name="action">The action to execute when invoking the command.</param>
        /// <param name="canExecute">An observable that dictates whether or not the command may execute. If not 
        /// specified, an observable is created that produces true.</param>
        /// <param name="defaultValue">A factory function to provide the return value for when the method fails to execute.</param>
        /// <param name="allowSimultaneousExecution">If true, multiple execution of this command may be performed.  If false, 
        /// then subsequent calls to ExecuteAsync return the defaultValue until the execution of the initial invocation completes.</param>
        public static IRxFunction<TInput, TOutput> Create<TInput, TOutput>(Func<TInput, TOutput> action, IObservable<bool> canExecute = null, Func<TOutput> defaultValue = null, bool allowSimultaneousExecution = false)
        {
            return RxFunction<TInput>.Create(action, canExecute, defaultValue, allowSimultaneousExecution);
        }

        /// <summary>
        /// Creates a command that consumes input and produces output.
        /// </summary>
        /// <param name="action">The action to execute when invoking the command.</param>
        /// <param name="canExecute">An observable that dictates whether or not the command may execute. If not 
        /// specified, an observable is created that produces true.</param>
        /// <param name="defaultValue">A factory function to provide the return value for when the method fails to execute.</param>
        /// <param name="allowSimultaneousExecution">If true, multiple execution of this command may be performed.  If false, 
        /// then subsequent calls to ExecuteAsync return the defaultValue until the execution of the initial invocation completes.</param>
        public static IRxFunction<TInput, TOutput> CreateAsync<TInput, TOutput>(Func<TInput, Task<TOutput>> action, IObservable<bool> canExecute = null, Func<TOutput> defaultValue = null, bool allowSimultaneousExecution = false)
        {
            return RxFunction<TInput>.CreateAsync(action, canExecute, defaultValue, allowSimultaneousExecution);
        }
    }

    /// <summary>
    /// This class facilitates creation functions with lambdas so the compiler can still infer the output type even though
    /// it can't infer the input type.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public static class RxFunction<TInput>
    {
        /// <summary>
        /// Creates a command that consumes input and produces output.  Non async version.
        /// </summary>
        /// <param name="action">The action to execute when invoking the command.</param>
        /// <param name="canExecute">An observable that dictates whether or not the command may execute. If not 
        /// specified, an observable is created that produces true.</param>
        /// <param name="defaultValue">A factory function to provide the return value for when the method fails to execute.</param>
        /// <param name="allowSimultaneousExecution">If true, multiple execution of this command may be performed.  If false, 
        /// then subsequent calls to ExecuteAsync return the defaultValue until the execution of the initial invocation completes.</param>
        public static IRxFunction<TInput, TOutput> Create<TOutput>(Func<TInput, TOutput> action, IObservable<bool> canExecute = null, Func<TOutput> defaultValue = null, bool allowSimultaneousExecution = false)
        {
            return new RxCommand<TInput, TOutput>(x => Task.FromResult(action(x)), canExecute, defaultValue, allowSimultaneousExecution);            
        }

        /// <summary>
        /// Creates a command that consumes input and produces output.
        /// </summary>
        /// <param name="action">The action to execute when invoking the command.</param>
        /// <param name="canExecute">An observable that dictates whether or not the command may execute. If not 
        /// specified, an observable is created that produces true.</param>
        /// <param name="defaultValue">A factory function to provide the return value for when the method fails to execute.</param>
        /// <param name="allowSimultaneousExecution">If true, multiple execution of this command may be performed.  If false, 
        /// then subsequent calls to ExecuteAsync return the defaultValue until the execution of the initial invocation completes.</param>
        public static IRxFunction<TInput, TOutput> CreateAsync<TOutput>(Func<TInput, Task<TOutput>> action, IObservable<bool> canExecute = null, Func<TOutput> defaultValue = null, bool allowSimultaneousExecution = false)
        {
            return new RxCommand<TInput, TOutput>(action, canExecute, defaultValue, allowSimultaneousExecution);
        }
    }
}
