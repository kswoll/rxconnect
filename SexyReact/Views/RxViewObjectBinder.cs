using System;
using System.Linq.Expressions;

namespace SexyReact.Views
{
    public struct RxViewObjectBinder<TModel, TModelValue>
        where TModel : IRxObject
    {
        public IRxViewObject<TModel> ViewObject { get; }
        public Expression<Func<TModel, TModelValue>> ModelProperty { get; }

        public RxViewObjectBinder(IRxViewObject<TModel> viewObject, Expression<Func<TModel, TModelValue>> modelProperty)
        {
            ViewObject = viewObject;
            ModelProperty = modelProperty;
        }
    }
}
