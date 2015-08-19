using System;
using UIKit;

namespace SexyReact.Views
{
    public static class RxInputBindExtensions
    {
        public static IDisposable Mate<TModel, TView>(
            this RxViewObjectBinder<TModel, string> binder,
            UITextField view
        )
            where TModel : IRxObject
        {
            return binder.Mate(view, x => x, x => x);
        }

        public static IDisposable Mate<TModel, TModelValue>(
            this RxViewObjectBinder<TModel, TModelValue> binder,
            UITextField view, 
            Func<TModelValue, string> toViewValue,
            Func<string, TModelValue> toModelValue
        )
            where TModel : IRxObject
        {
            return binder.Mate(view, x => toModelValue(x.Text), (x, value) => x.Text = toViewValue(value), UIControlEvent.EditingChanged);
        }
    }
}

