using System;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using SexyReact.Utils;

namespace SexyReact.Views
{
    public static class RxEventBindExtensions
    {
        public static IDisposable Mate<TModel, TModelValue, TView, TEventHandler>(
            this RxViewObjectBinder<TModel, TModelValue> binder,
            TView view, 
            Func<TView, TModelValue> getter,
            Action<TView, TModelValue> setter,
            Func<Action, TEventHandler> eventHandlerFactory,
            Action<TView, TEventHandler> add,
            Action<TView, TEventHandler> remove
        )
            where TModel : IRxObject
        {
            var toSubscription = binder.To(view, setter);

            var modelSetter = binder.CreateModelPropertySetter();
            var propagate = new Action(() => modelSetter(getter(view)));
            var listener = eventHandlerFactory(propagate);
            add(view, listener);

            return new CompositeDisposable(toSubscription, new Unsubscribe<TView, TEventHandler>(view, remove, listener));
        }

        public static IDisposable Mate<TModel, TView, TValue, TEventHandler>(
            this RxViewObjectBinder<TModel, TValue> binder,
            TView view, 
            Expression<Func<TView, TValue>> viewProperty,            
            Func<Action<TValue>, TEventHandler> eventHandlerFactory,
            Action<TView, TEventHandler> add,
            Action<TView, TEventHandler> remove
        )
            where TModel : IRxObject
        {
            return binder.Mate(view, viewProperty, eventHandlerFactory, add, remove, x => x, x => x);
        }

        public static IDisposable Mate<TModel, TModelValue, TView, TViewValue, TEventHandler>(
            this RxViewObjectBinder<TModel, TModelValue> binder,
            TView view, 
            Expression<Func<TView, TViewValue>> viewProperty,            
            Func<Action<TViewValue>, TEventHandler> eventHandlerFactory,
            Action<TView, TEventHandler> add,
            Action<TView, TEventHandler> remove,
            Func<TModelValue, TViewValue> toViewValue,
            Func<TViewValue, TModelValue> toModelValue
        )
            where TModel : IRxObject
        {
            var toSubscription = binder.To(view, viewProperty, toViewValue);

            var modelSetter = binder.CreateModelPropertySetter();
            var propagate = new Action<TViewValue>(x => 
            {
                modelSetter(toModelValue(x));
            });
            var listener = eventHandlerFactory(propagate);
            add(view, listener);

            return new CompositeDisposable(toSubscription, new Unsubscribe<TView, TEventHandler>(view, remove, listener));
        }

        public static IDisposable To<TModel, TModelValue, TView, TEventHandler>(
            this RxViewObjectBinder<TModel, TModelValue> binder,
            TView view, 
            Func<Action, TEventHandler> eventHandlerFactory,
            Action<TView, TEventHandler> add,
            Action<TView, TEventHandler> remove
        )
            where TModel : IRxObject
            where TModelValue : IRxCommand
        {
            IRxCommand command = null;
            Action propagate = () => command.InvokeAsync();
            var listener = eventHandlerFactory(propagate);
            add(view, listener);
            var buttonDisposable = new ActionDisposable(() => remove(view, listener));
            var subscription = binder.ObserveModelProperty().Subscribe(x => 
            {
                command = x;
            });
            return new CompositeDisposable(subscription, buttonDisposable);
        }

        private struct Unsubscribe<TView, TEventHandler> : IDisposable
        {
            private readonly TView view;
            private readonly Action<TView, TEventHandler> remove;
            private readonly TEventHandler listener;

            public Unsubscribe(TView view, Action<TView, TEventHandler> remove, TEventHandler listener)
            {
                this.view = view;
                this.remove = remove;
                this.listener = listener;
            }

            public void Dispose()
            {
                remove(view, listener);
            }
        }
    }
}
