using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Reactive.Linq;

namespace SexyReact
{
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
        private Lazy<IObservable<bool>> canExecute;
        private Func<TInput, Task<TOutput>> action;
        private Lazy<Subject<TOutput>> subject = new Lazy<Subject<TOutput>>(() => new Subject<TOutput>());
        private Lazy<ReplaySubject<bool>> isExecuting = new  Lazy<ReplaySubject<bool>>(() => new ReplaySubject<bool>(1));
        private object lockObject = new object();
        private bool isSubscribedToCanExecute;
        private bool isAllowedToExecute;
        private bool requireOneAtATime = true;

        /// <summary>
        /// You are free to create commands using this constructor, but you may find it more convenient to use one of 
        /// the factory methods in RxCommand and RxFunction.
        /// </summary>
        /// <param name="action">The action to execute when invoking the command.</param>
        /// <param name="canExecute">An observable that dictates whether or not the command may execute. If not 
        /// specified, an observable is created that produces true.</param>
        public RxCommand(Func<TInput, Task<TOutput>> action, IObservable<bool> canExecute = null)
        {
            this.action = action;
            if (canExecute == null)
                this.canExecute = new Lazy<IObservable<bool>>(() => isExecuting.Value);
            else
                this.canExecute = new Lazy<IObservable<bool>>(() => canExecute.SelectMany(x => !isExecuting.IsValueCreated ? Observable.Return(x) : IsExecuting.Select(y => x && y)));
        }

        public IObservable<bool> CanExecute
        {
            get { return canExecute.Value; }
        }

        public IObservable<bool> IsExecuting
        {
            get { return isExecuting.Value; }
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
            if (requireOneAtATime)
            {
                lock (lockObject)
                {
                    if (!isSubscribedToCanExecute)
                    {
                        CanExecute.Subscribe(UpdateIsAllowedToExecute);
                        isSubscribedToCanExecute = true;
                    }
                    if (!isAllowedToExecute)
                    {
                        return default(TOutput);
                    }
                    isAllowedToExecute = false;
                }                
            }

            if (!await CanExecute.LastAsync())
                return default(TOutput);

            isExecuting.Value.OnNext(true);

            var result = await action(input);
            if (subject.IsValueCreated)
                subject.Value.OnNext(result);

            isExecuting.Value.OnNext(false);

            return result;
        }

        private void UpdateIsAllowedToExecute(bool value)
        {
            lock (lockObject)
            {
                isAllowedToExecute = value;
            }
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

