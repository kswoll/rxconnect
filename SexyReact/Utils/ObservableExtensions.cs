using System;
using System.Reactive.Subjects;
using System.Reactive;

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

        public struct EmptyDisposable : IDisposable
        {
            public void Dispose() 
            {
            }
        }
    }
}

