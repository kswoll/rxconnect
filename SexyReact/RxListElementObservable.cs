using System;
using System.Linq.Expressions;

namespace SexyReact
{
    public struct RxListElementObservable<T, TValue> : IObservable<RxListObservedElement<T, TValue>> where T : IRxObject
    {
        private readonly IRxList<T> list;
        private readonly Expression<Func<T, TValue>> selector;
        private bool onlyChanges;

        public RxListElementObservable(IRxList<T> list, Expression<Func<T, TValue>> selector, bool onlyChanges) : this()
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            this.list = list;
            this.selector = selector;
            this.onlyChanges = onlyChanges;
        }

        public IDisposable Subscribe(IObserver<RxListObservedElement<T, TValue>> observer)
        {
            return new Observer(list, selector, observer, onlyChanges);
        }

        private class Observer : IDisposable
        {
            private readonly IObserver<RxListObservedElement<T, TValue>> observer;
            private readonly IRxReadOnlyList<IDisposable> subscriptions;
            private bool disposed;

            public Observer(IRxList<T> list, Expression<Func<T, TValue>> selector, IObserver<RxListObservedElement<T, TValue>> observer, bool onlyChanges)
            {
                this.observer = observer;
                if (onlyChanges)
                {
                    if (selector == null)
                        subscriptions = list.Derive(x => x.Changed.Subscribe(y => OnElementChanged(x, default(TValue))), x => x.Dispose());
                    else
                        subscriptions = list.Derive(x => x.ObservePropertyChange(selector).Subscribe(y => OnElementChanged(x, y)), x => x.Dispose());
                }
                else
                {
                    subscriptions = list.Derive(x => x.ObserveProperty(selector).Subscribe(y => OnElementChanged(x, y)), x => x.Dispose());
                }
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