using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Text;

namespace SexyReact.Views
{
    public static class RxEventBindExtensions
    {
        public static IDisposable Mate<TModel, TView, TValue, TEventHandler>(
            this RxViewObjectBinder<TModel, TValue> binder,
            TView view, 
            Expression<Func<TView, TValue>> viewProperty,
            Action<TView, TEventHandler> add,
            Action<TView, TEventHandler> remove
        )
            where TView : IRxObject
            where TModel : IRxObject
        {
            var connectDisposable = binder.To(view, viewProperty);
            Lazy<Action<TValue>> setValue = new Lazy<Action<TValue>>(() => binder.ViewObject.CreateModelPropertySetter(view, binder.ModelProperty));

            var result = view
                .ObserveProperty(viewProperty)
                .Subscribe(x => setValue.Value(x));

            return new CompositeDisposable(connectDisposable, result);
        }
    }
}
