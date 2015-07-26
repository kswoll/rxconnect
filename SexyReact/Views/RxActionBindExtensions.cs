using System;
using SexyReact.Utils;

namespace SexyReact.Views
{
    public static class RxActionBindExtensions
    {
        public static IDisposable To<TModel, TModelValue>(
            this RxViewObjectBinder<TModel, TModelValue> binder,
            Action<TModelValue> setter
        )
            where TModel : IRxObject
        {
            var result = binder
                .ObserveModelProperty()
                .SubscribeOnUiThread(x => 
                {
                    setter(x);
                });
            return result;
        }

        public static IDisposable To<TModel, TModelValue, TView>(
            this RxViewObjectBinder<TModel, TModelValue> binder,
            TView view,
            Action<TView, TModelValue> setter
        )
            where TModel : IRxObject
        {
            var result = binder
                .ObserveModelProperty()
                .SubscribeOnUiThread(x => setter(view, x));
            return result;
        }
    }
}
