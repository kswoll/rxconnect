using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;
using System.Reactive.Disposables;
using SexyReact.Utils;

namespace SexyReact.Views
{
    public static class RxViewObjectExtensions
    {
        public static RxViewObjectBinder<TModel, TModelValue> Bind<TModel, TModelValue>(
            this IRxViewObject<TModel> viewObject,
            Expression<Func<TModel, TModelValue>> modelProperty
        )
            where TModel : IRxObject
        {
            return new RxViewObjectBinder<TModel, TModelValue>(viewObject, modelProperty);
        }
    }
}

