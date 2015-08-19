using System;
using UIKit;
using System.Reactive.Disposables;
using System.Linq.Expressions;
using SexyReact.Utils;

namespace SexyReact.Views
{
    public static class UIControlEventBindExtensions
    {
        public static IDisposable Mate<TModel, TModelValue, TView>(
            this RxViewObjectBinder<TModel, TModelValue> binder,
            TView view, 
            Func<TView, TModelValue> getter,
            Action<TView, TModelValue> setter,
            UIControlEvent controlEvent
        )
            where TModel : IRxObject
            where TView : UIControl
        {
            var toSubscription = binder.To(view, setter);

            var modelSetter = binder.CreateModelPropertySetter();
            var propagate = new Action(() => modelSetter(getter(view)));
            var listener = new EventHandler((sender, args) => propagate());

            view.AddTarget(listener, controlEvent);

            return new CompositeDisposable(toSubscription, new Unsubscribe(view, listener, controlEvent));
        }

        public static IDisposable Mate<TModel, TValue, TView>(
            this RxViewObjectBinder<TModel, TValue> binder,
            TView view, 
            Expression<Func<TView, TValue>> viewProperty,
            UIControlEvent controlEvent
        )
            where TModel : IRxObject
            where TView : UIControl
        {
            return binder.Mate(view, viewProperty, controlEvent, x => x, x => x);
        }

        public static IDisposable Mate<TModel, TModelValue, TView, TViewValue>(
            this RxViewObjectBinder<TModel, TModelValue> binder,
            TView view, 
            Expression<Func<TView, TViewValue>> viewProperty,
            UIControlEvent controlEvent,
            Func<TModelValue, TViewValue> toViewValue,
            Func<TViewValue, TModelValue> toModelValue
        )
            where TModel : IRxObject
            where TView : UIControl
        {
            var toSubscription = binder.To(view, viewProperty, toViewValue);

            var viewGetter = viewProperty.Compile();
            var modelSetter = binder.CreateModelPropertySetter();
            var propagate = new Action(() => 
            {
                modelSetter(toModelValue(viewGetter(view)));
            });
            var listener = new EventHandler((sender, args) => propagate());
            view.AddTarget(listener, controlEvent);

            return new CompositeDisposable(toSubscription, new Unsubscribe(view, listener, controlEvent));
        }

        public static IDisposable To<TModel, TModelValue, TView>(
            this RxViewObjectBinder<TModel, TModelValue> binder,
            TView view,
            UIControlEvent controlEvent
        )
            where TModel : IRxObject
            where TModelValue : IRxCommand
            where TView : UIControl
        {
            IRxCommand command = null;
            Action propagate = () => command.ExecuteAsync();
            var listener = new EventHandler((sender, args) => propagate());
            view.AddTarget(listener, controlEvent);
            var buttonDisposable = new Unsubscribe(view, listener, controlEvent);
            var subscription = binder.ObserveModelProperty().Subscribe(x => 
            {
                command = x;
            });
            return new CompositeDisposable(subscription, buttonDisposable);
        }

        private struct Unsubscribe : IDisposable
        {
            private readonly UIControl view;
            private readonly EventHandler listener;
            private readonly UIControlEvent controlEvent;

            public Unsubscribe(UIControl view, EventHandler listener, UIControlEvent controlEvent)
            {
                this.view = view;
                this.listener = listener;
                this.controlEvent = controlEvent;
            }

            public void Dispose()
            {
                view.RemoveTarget(listener, controlEvent);
            }
        }
    }
}

