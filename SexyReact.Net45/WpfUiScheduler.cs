using System;
using System.Reactive.Concurrency;
using System.Threading;
using System.Windows.Threading;

namespace SexyReact
{
    public class WpfUiScheduler : IScheduler
    {
        public static WpfUiScheduler Instance { get; } = new WpfUiScheduler();

        private DispatcherScheduler dispatcherScheduler;
        
        private WpfUiScheduler()
        {
        }

        private IScheduler ActualScheduler 
        {
            get
            {
                if (dispatcherScheduler != null)
                    return dispatcherScheduler;

                var dispatcher = Dispatcher.FromThread(Thread.CurrentThread);
                if (dispatcher == null)
                    return CurrentThreadScheduler.Instance;
                else
                    return dispatcherScheduler = DispatcherScheduler.Current;                
            }
        }

        public DateTimeOffset Now => ActualScheduler.Now;

        public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
        {
            return ActualScheduler.Schedule(state, action);
        }

        public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            return ActualScheduler.Schedule(state, dueTime, action);
        }

        public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            return ActualScheduler.Schedule(state, dueTime, action);
        }
    }
}