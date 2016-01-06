using System;
using System.Collections.Generic;
using System.Linq.Expressions;

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

