using System;
using SexyReact.Views;
using System.Linq.Expressions;
using UIKit;

namespace SexyReact.Ios
{
    public static class IosViewObjectExtensions
    {
        public static IDisposable Connect<TViewTarget, TModel, TModelItem>(
            this IRxViewObject<TModel> view, 
            TViewTarget viewTarget, 
            Expression<Func<TViewTarget, UITableView>> viewProperty,
            Expression<Func<TModel, RxList<TModelItem>>> modelProperty
        )
            where TModel : IRxObject
        {
            return null;
        }
    }
}

