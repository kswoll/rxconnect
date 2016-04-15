using System;
using System.Reactive.Subjects;
using System.Reactive;
using System.Threading.Tasks;

namespace SexyReact.Utils
{
    public static class ObservableExtensions
    {
        public static IObservable<T> ObserveOnUiThread<T>(this IObservable<T> observable)
        {
            var subject = new Subject<T>();
            observable.Subscribe(x => RxApp.UiScheduler.Schedule(default(Unit), (scheduler, state) => 
            {
                subject.OnNext(x);
                return new EmptyDisposable();
            }));
            return subject;
        }

        /// <summary>
        /// Allows you to conveniently perform an async operation on your subscription.  The caveat is that the body of the 
        /// subscription will return immediately without waiting for the async operation to complete.  
        /// </summary>
        public static IDisposable SubscribeAsync<T>(this IObservable<T> observable, Func<T, Task> subscriber)
        {
            return observable.Subscribe(x =>
            {
                Task.Run(async () =>
                {
                    await subscriber(x);
                });
            });
        }

        /// <summary>
        /// Ensures that the subscriber's work happens on the UI thread.
        /// </summary>
        public static IDisposable SubscribeOnUiThread<T>(this IObservable<T> observable, Action<T> subscriber)
        {
            return observable.Subscribe(x =>
            {
                RxApp.UiScheduler.Schedule(default(Unit), (_, __) => 
                {
                    subscriber(x);
                    return new EmptyDisposable();
                });
            });
        }

        public static void SubscribeOnce<T>(this IObservable<T> observable, Action<T> subscriber)
        {
            IDisposable subscription = null;
            bool needsToDispose = false;
            Action<T> instrumentedSubscriber = x =>
            {
                subscriber(x);
                if (subscription != null)
                    subscription.Dispose();
                else
                    needsToDispose = true;
            };
            subscription = observable.Subscribe(instrumentedSubscriber);
            if (needsToDispose)
                subscription.Dispose();
        }

        public struct EmptyDisposable : IDisposable
        {
            public void Dispose() 
            {
            }
        }
    }
}

