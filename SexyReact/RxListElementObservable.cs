using System;
using System.Linq.Expressions;

namespace SexyReact
{
    public struct RxListElementObservable<T, TValue> : IObservable<RxListObservedElement<T, TValue>> where T : IRxObject
    {
        private readonly IRxList<T> list;
        private readonly Expression<Func<T, TValue>> selector;

        public RxListElementObservable(IRxList<T> list, Expression<Func<T, TValue>> selector) : this()
        {
            this.list = list;
            this.selector = selector;
        }

        public IDisposable Subscribe(IObserver<RxListObservedElement<T, TValue>> observer)
        {
            return new Observer(list, selector, observer);
        }

        private class Observer : IDisposable
        {
            private readonly IObserver<RxListObservedElement<T, TValue>> observer;
            private readonly IRxReadOnlyList<IDisposable> subscriptions;
            private bool disposed;

            public Observer(IRxList<T> list, Expression<Func<T, TValue>> selector, IObserver<RxListObservedElement<T, TValue>> observer)
            {
                this.observer = observer;
                subscriptions = list.Derive(x => x.ObserveProperty(selector).Subscribe(y => OnElementChanged(x, y)), x => x.Dispose());
            }

            private void OnElementChanged(T element, TValue value)
            {
                observer.OnNext(new RxListObservedElement<T, TValue>(element, value));
            }

            public void Dispose()
            {
                if (!disposed)
                {
                    disposed = true;
                    foreach (var subscription in subscriptions)
                        subscription.Dispose();
                    subscriptions.Dispose();
                }
            }
        }
    }
}