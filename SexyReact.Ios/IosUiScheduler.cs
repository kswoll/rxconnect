/// <summary>
/// Taken from: 
/// https://github.com/reactiveui/ReactiveUI/blob/bdb3c792b8c9033751b887615add11acc42475e7/ReactiveUI/Cocoa/NSRunloopScheduler.cs
/// 
/// License: https://raw.githubusercontent.com/reactiveui/ReactiveUI/bdb3c792b8c9033751b887615add11acc42475e7/COPYING
/// </summary>

using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using Foundation;
using CoreFoundation;

namespace SexyReact.Ios
{
    public class IosUiScheduler : IScheduler
    {
        public static IosUiScheduler Instance = new IosUiScheduler();
        
        private IosUiScheduler()
        {
        }

        public DateTimeOffset Now 
        {
            get { return DateTimeOffset.Now; }
        }

        public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
        {
            var innerDisp = new SingleAssignmentDisposable();

            DispatchQueue.MainQueue.DispatchAsync(() => 
            {
                if (!innerDisp.IsDisposed) innerDisp.Disposable = action(this, state);
            });

            return innerDisp;
        }

        public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            if (dueTime <= Now) 
            {
                return Schedule(state, action);
            }

            return Schedule(state, dueTime - Now, action);
        }

        public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            var innerDisp = Disposable.Empty;
            bool isCancelled = false;

            var timer = NSTimer.CreateScheduledTimer(dueTime, _ => 
            {
                if (!isCancelled) innerDisp = action(this, state);
            });

            return Disposable.Create(() => 
            {
                isCancelled = true;
                timer.Invalidate();
                innerDisp.Dispose();
            });
        }
    }
}

