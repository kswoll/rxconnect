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
            var result = binder.ViewObject
                .ObserveModelProperty(binder.ModelProperty)
                .SubscribeOnUiThread(x => setter(x));
            return result;
        }
    }
}
