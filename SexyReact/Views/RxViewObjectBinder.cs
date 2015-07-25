using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SexyReact.Views
{
    public struct RxViewObjectBinder<TModel, TModelValue>
        where TModel : IRxObject
    {
        private readonly IRxViewObject<TModel> viewObject;
        private readonly Expression<Func<TModel, TModelValue>> modelProperty;

        public RxViewObjectBinder(IRxViewObject<TModel> viewObject, Expression<Func<TModel, TModelValue>> modelProperty)
        {
            this.viewObject = viewObject;
            this.modelProperty = modelProperty;
        }

        public IRxViewObject<TModel> ViewObject
        {
            get { return viewObject; }
        }

        public Expression<Func<TModel, TModelValue>> ModelProperty
        {
            get { return modelProperty; }
        }
    }
}
