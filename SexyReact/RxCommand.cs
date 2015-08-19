using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive;

namespace SexyReact
{
    public class RxCommand<TInput> : 
        RxCommand<TInput, Unit>, 
        IRxCommand,
        IRxCommand<TInput>
    {
        public RxCommand(Func<TInput, Task<Unit>> action, IObservable<bool> canExecute = null, Func<Unit> defaultValue = null, bool allowSimultaneousExecution = false) 
            : base(action, canExecute, defaultValue, allowSimultaneousExecution)
        {
        }

        Task IRxCommand.ExecuteAsync()
        {
            return ExecuteAsync(default(TInput));
        }

        Task IRxCommand<TInput>.ExecuteAsync(TInput input)
        {
            return ExecuteAsync(input);
        }
    }

    public class RxCommand<TInput, TOutput> : 
        IRxFunction<TOutput>, 
        IRxFunction<TInput, TOutput>
    {
        private Lazy<IObservable<bool>> canExecute;
        private Func<TInput, Task<TOutput>> action;
        private Lazy<Subject<TOutput>> subject = new Lazy<Subject<TOutput>>(() => new Subject<TOutput>());
        private Lazy<ReplaySubject<bool>> isExecuting = new  Lazy<ReplaySubject<bool>>(() => 
        {
            var subject = new ReplaySubject<bool>(1);
            subject.OnNext(false);      // Initialize with a default value
            return subject;
        });
        private object lockObject = new object();
        private bool isSubscribedToCanExecute;
        private bool isAllowedToExecute = true;
        private Func<TOutput> defaultValue;

        /// <summary>
        /// You are free to create commands using this constructor, but you may find it more convenient to use one of 
        /// the factory methods in RxCommand and RxFunction.
        /// </summary>
        /// <param name="action">The action to execute when invoking the command.</param>
        /// <param name="canExecute">An observable that dictates whether or not the command may execute. If not 
        /// specified, an observable is created that produces true.</param>
        /// <param name="defaultValue">A factory function to provide the return value for when the method fails to execute.</param>
        /// <param name="allowSimultaneousExecution">If true, multiple execution of this command may be performed.  If false, 
        /// then subsequent calls to ExecuteAsync return the defaultValue until the execution of the initial invocation completes.</param>
        public RxCommand(Func<TInput, Task<TOutput>> action, IObservable<bool> canExecute = null, Func<TOutput> defaultValue = null, bool allowSimultaneousExecution = false)
        {
            this.action = action;
            this.defaultValue = defaultValue ?? (() => default(TOutput));
            
            Func<IObservable<bool>> canExecuteFactory;
            if (allowSimultaneousExecution)
            {
                if (canExecute == null)
                    canExecuteFactory = () => Observable.Return(true);
                else
                    canExecuteFactory = () => canExecute;
            }
            else
            {
                if (canExecute == null)
                    canExecuteFactory = () => IsExecuting.Select(x => !x);
                else
                    canExecuteFactory = () => IsExecuting.SelectMany(x => canExecute.Select(y => !x && y));
            }

            this.canExecute = new Lazy<IObservable<bool>>(canExecuteFactory);
        }

        public IObservable<bool> CanExecute => canExecute.Value;
        public IObservable<bool> IsExecuting => isExecuting.Value;

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
            lock (lockObject)
            {
                if (!isSubscribedToCanExecute)
                {
                    CanExecute.Subscribe(UpdateIsAllowedToExecute);
                    isSubscribedToCanExecute = true;
                }
                if (!isAllowedToExecute)
                {
                    return defaultValue();
                }
            }                

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

