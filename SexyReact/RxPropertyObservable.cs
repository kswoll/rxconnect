using System;
using System.Collections.Generic;
using System.Reflection;

namespace SexyReact
{
    public struct RxPropertyObservable<TValue> : IObservable<TValue>
    {
        private static readonly MethodInfo observePropertyAsObjectMethod = typeof(RxObjectExtensions).GetMethod("ObservePropertyAsObject");

        private readonly IRxObject source;
        private readonly IReadOnlyList<PropertyInfo> propertyPath;

        public RxPropertyObservable(IRxObject source, params PropertyInfo[] propertyPath) : this()
        {
            this.source = source;
            this.propertyPath = propertyPath;
        }

        public IDisposable Subscribe(IObserver<TValue> observer)
        {
            return new RxPropertyObserver(source, propertyPath, observer);
        }

        private class RxPropertyObserver : IObserver<TValue>, IDisposable
        {
            private IObserver<TValue> observer;
            private readonly IRxObject source;
            private readonly IReadOnlyList<PropertyInfo> propertyPath;
            private TValue lastValue;
            private bool hasEmittedInitialValue;
            private IDisposable subscription;
            private PathObserver rootPathObserver;

            public RxPropertyObserver(IRxObject source, IReadOnlyList<PropertyInfo> propertyPath, IObserver<TValue> observer)
            {
                this.source = source;
                this.propertyPath = propertyPath;
                this.observer = observer;

                if (propertyPath.Count == 1)
                {
                    subscription = source.ObserveProperty<TValue>(propertyPath[0]).Subscribe(this);
                }
                else
                {
                    var firstPropertyInfo = propertyPath[0];
                    var firstObserveProperty = observePropertyAsObjectMethod.MakeGenericMethod(firstPropertyInfo.PropertyType);
                    var firstObservable = (IObservable<object>)firstObserveProperty.Invoke(null, new object[] { source, firstPropertyInfo });
                    subscription = firstObservable.Subscribe(rootPathObserver = new PathObserver(this, 1));
                }
            }

            private class PathObserver : IObserver<object>, IDisposable
            {
                private readonly RxPropertyObserver parent;
                private IDisposable subscription;
                private int pathIndex;
                private PropertyInfo property;
                private MethodInfo observeProperty;
                private PathObserver nextPathObserver;
                private object lastValue;
                private bool hasEmittedInitialValue;

                public PathObserver(RxPropertyObserver parent, int pathIndex)
                {
                    this.parent = parent;
                    this.pathIndex = pathIndex;
                }

                public void OnNext(object value)
                {
                    var emit = false;
                    lock (parent)
                    {
                        if (!hasEmittedInitialValue)
                            emit = true;
                        if (!Equals(lastValue, value))
                            emit = true;
                        hasEmittedInitialValue = true;
                        lastValue = value;
                    }
                    if (!emit)
                        return;

                    if (value == null)
                    {
                        lock (parent)
                        {
                            if (subscription != null)
                            {
                                subscription.Dispose();
                            }
                        }
                        parent.OnNext(default(TValue));
                    }
                    else
                    {
                        lock (parent)
                        {
                            if (property == null)
                            {
                                property = parent.propertyPath[pathIndex];
                                observeProperty = observePropertyAsObjectMethod.MakeGenericMethod(property.PropertyType);                                
                            }
                            if (subscription != null)
                            {
                                subscription.Dispose();
                            }
                            var rxObject = (IRxObject)value;
                            if (pathIndex < parent.propertyPath.Count - 1)
                            {
                                nextPathObserver = new PathObserver(parent, pathIndex + 1);
                                var observable = (IObservable<object>)observeProperty.Invoke(null, new object[] { rxObject, property });
                                subscription = observable.Subscribe(nextPathObserver);                                
                            }
                            else
                            {
                                subscription = rxObject.ObserveProperty<TValue>(property).Subscribe(parent);
                            }
                        }
                    }
                }

                public void OnError(Exception error)
                {
                    parent.OnError(error);
                }

                public void OnCompleted()
                {
                }

                public void Dispose()
                {
                    if (subscription != null)
                    {
                        subscription.Dispose();
                    }
                    if (nextPathObserver != null)
                    {
                        nextPathObserver.Dispose();
                    }
                }
            }

            public void OnNext(TValue value)
            {
                var emit = false;
                lock (this)
                {
                    if (!hasEmittedInitialValue)
                        emit = true;
                    else if (!Equals(lastValue, value))
                        emit = true;
                    hasEmittedInitialValue = true;
                    lastValue = value;
                }
                if (emit)
                {
                    observer.OnNext(value);
                }
            }

            public void OnError(Exception error)
            {
                observer.OnError(error);
            }

            public void OnCompleted()
            {
                observer.OnCompleted();
            }

            public void Dispose()
            {
                subscription.Dispose();
                if (rootPathObserver != null)
                {
                    rootPathObserver.Dispose();
                }
            }
        }
    }
}
