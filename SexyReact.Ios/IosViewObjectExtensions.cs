using System;
using SexyReact.Views;
using System.Linq.Expressions;
using UIKit;

namespace SexyReact.Ios
{
    public static class IosViewObjectExtensions
    {
        public static IDisposable Connect<TViewTarget, TModel, TModelItem, TCell>(
            this IRxViewObject<TModel> view, 
            UITableView tableView, 
            Expression<Func<TModel, RxList<TModelItem>>> modelProperty,
            Func<TModelItem, TCell> cellFactory
        )
            where TModel : IRxObject
            where TModelItem : IRxObject
            where TCell : RxTableViewCell<TModelItem>
        {
            var tableSource = new RxTableViewSource<RxList<TModelItem>, TModelItem>(tableView, x => x, (section, item) => cellFactory(item));
            var result = view
                .ObserveModelProperty(modelProperty)
                .Subscribe(x => tableSource.Data = new RxList<RxList<TModelItem>>(x));
            return result;
        }
    }
}

