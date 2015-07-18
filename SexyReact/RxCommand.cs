using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Reactive;

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
        public static IRxFunction<TInput, TOutput> CreateFunction<TOutput>(Func<TInput, TOutput> action)
        {
            return RxCommand.CreateFunction(action);
        }

        /// <summary>
        /// Creates a command that consumes input and produces output.
        /// </summary>
        public static IRxFunction<TInput, TOutput> CreateFunction<TOutput>(Func<TInput, Task<TOutput>> action)
        {
            return RxCommand.CreateFunction(action);
        }
    }

    public class RxCommand<TInput, TOutput> : 
        IRxCommand, 
        IRxCommand<TInput>, 
        IRxFunction<TOutput>, 
        IRxFunction<TInput, TOutput>
    {
        private Func<TInput, Task<TOutput>> action;
        private Lazy<Subject<TOutput>> subject = new Lazy<Subject<TOutput>>(() => new Subject<TOutput>());

        public RxCommand(Func<TInput, Task<TOutput>> action)
        {
            this.action = action;
        }

        public IDisposable Subscribe(IObserver<TOutput> observer)
        {
            return subject.Value.Subscribe(observer);
        }

        public TOutput Execute(TInput input)
        {
            return ExecuteAsync(input).Result;
        }

        /// <summary>
        /// Executes the task asynchronously.  The observable represented by this command emits its next value *before*
        /// this method completes and returns its value.
        /// </summary>
        public async Task<TOutput> ExecuteAsync(TInput input) 
        {
            var result = await action(input);
            if (subject.IsValueCreated)
                subject.Value.OnNext(result);
            return result;
        }

        Task IRxCommand.ExecuteAsync()
        {
            return ExecuteAsync(default(TInput));
        }

        Task IRxCommand<TInput>.ExecuteAsync(TInput input)
        {
            return ExecuteAsync(input);
        }

        Task<TOutput> IRxFunction<TOutput>.ExecuteAsync()
        {
            return ExecuteAsync(default(TInput));
        }

        Task<TOutput> IRxFunction<TInput, TOutput>.ExecuteAsync(TInput input) 
        {
            return ExecuteAsync(input);
        }
    }
}

