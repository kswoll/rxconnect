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
        public static IRxCommand CreateCommand(Action action)
        {
            return CreateCommand(() => 
            {
                action();
                return Task.FromResult(default(Unit));
            });
        }

        /// <summary>
        /// Creates a command that consumes no input and produces no output.
        /// </summary>
        public static IRxCommand CreateCommand(Func<Task> action)
        {
            return new RxCommand<Unit, Unit>(async x => 
            {
                await action();
                return default(Unit);
            });
        }

        /// <summary>
        /// Creates a command that consumes input, but produces no output.  Non async version.
        /// </summary>
        public static IRxCommand<TInput> CreateCommand<TInput>(Action<TInput> action)
        {
            return new RxCommand<TInput, Unit>(x =>
            {
                action(x);
                return Task.FromResult(default(Unit));
            });
        }

        /// <summary>
        /// Creates a command that consumes input, but produces no output.
        /// </summary>
        public static IRxCommand<TInput> CreateCommand<TInput>(Func<TInput, Task> action)
        {
            return new RxCommand<TInput, Unit>(async x =>
            {
                await action(x);
                return default(Unit);
            });
        }

        /// <summary>
        /// Creates a command that consumes no input, but produces output.  Non async version.
        /// </summary>
        public static IRxFunction<TOutput> CreateFunction<TOutput>(Func<TOutput> action)
        {
            return new RxCommand<Unit, TOutput>(x => Task.FromResult(action()));
        }

        /// <summary>
        /// Creates a command that consumes no input, but produces output.
        /// </summary>
        public static IRxFunction<TOutput> CreateFunction<TOutput>(Func<Task<TOutput>> action)
        {
            return new RxCommand<Unit, TOutput>(x => action());
        }

        /// <summary>
        /// Creates a command that consumes input and produces output.  Non async version.
        /// </summary>
        public static IRxFunction<TInput, TOutput> CreateFunction<TInput, TOutput>(Func<TInput, TOutput> action)
        {
            return new RxCommand<TInput, TOutput>(x => Task.FromResult(action(x)));
        }

        /// <summary>
        /// Creates a command that consumes input and produces output.
        /// </summary>
        public static IRxFunction<TInput, TOutput> CreateFunction<TInput, TOutput>(Func<TInput, Task<TOutput>> action)
        {
            return new RxCommand<TInput, TOutput>(action);
        }
    }
}
