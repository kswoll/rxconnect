using System;
using SexyReact.Views;
using System.Linq.Expressions;
using UIKit;
using SexyReact.Utils;
using System.Reactive.Disposables;

namespace SexyReact.Ios
{
    public static class IosViewObjectExtensions
    {
        /// <summary>
        /// Connects an RxList<T> to a UITableView such that the rows in the UITableView will update based on changes to 
        /// the list.
        /// </summary>
        /// <returns>A disposable to unsubscribe notifications.  Generally, you can safely ignore this.</returns>
        /// <param name="view">The current view controller as "this"</param>
        /// <param name="tableView">The UITableView to which you are trying to connect.</param>
        /// <param name="modelProperty">The RxList<T> that provides the data to the UITableView.</param>
        /// <param name="cellFactory">A factory function to create new cells.</param>
        /// <typeparam name="TModel">The type of your model (should be inferred).</typeparam>
        /// <typeparam name="TModelItem">The type of each item in your list (should be inferred).</typeparam>
        /// <typeparam name="TCell">The type of the cell (should be inferred).</typeparam>
        public static IDisposable To<TModel, TModelItem, TCell>(
            this RxViewObjectBinder<TModel, RxList<TModelItem>> binder,
            UITableView tableView, 
            Func<TModelItem, TCell> cellFactory
        )
            where TModel : IRxObject
            where TModelItem : IRxObject
            where TCell : RxTableViewCell<TModelItem>
        {
            var tableSource = new RxTableViewSource<RxList<TModelItem>, TModelItem, TCell>(tableView, x => x, (section, item) => cellFactory(item));
            var result = binder
                .ObserveModelProperty()
                .Subscribe(x => tableSource.Data = x == null ? null : new RxList<RxList<TModelItem>>(x));
            tableView.Source = tableSource;
            return result;
        }

        public static IDisposable To<TModel, TModelValue>(
            this RxViewObjectBinder<TModel, TModelValue> binder,
            UIButton button
        )
            where TModel : IRxObject
            where TModelValue : IRxCommand
        {
            return binder.To(button, x => new EventHandler((sender, e) => x()), (x, l) => x.TouchUpInside += l, (x, l) => x.TouchUpInside -= l);
        }

        public static IDisposable To<TModel, TModelValue>(
            this RxViewObjectBinder<TModel, TModelValue> binder,
            UIBarButtonItem button
        )
            where TModel : IRxObject
            where TModelValue : IRxCommand
        {
            return binder.To(button, x => new EventHandler((sender, e) => x()), (x, l) => x.Clicked += l, (x, l) => x.Clicked -= l);
        }

        public static IDisposable Mate<TModel>(
            this RxViewObjectBinder<TModel, string> binder,
            UITextField textField
        )
            where TModel : IRxObject
        {
            return binder.Mate(textField, x => x.Text, x => new EventHandler((sender, args) => x()), (x, l) => x.Ended += l, (x, l) => x.Ended -= l);
        }
    }
}

