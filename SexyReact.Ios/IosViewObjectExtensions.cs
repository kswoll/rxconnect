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
        public static IDisposable ConnectTableView<TModel, TModelItem, TCell>(
            this IRxViewObject<TModel> view, 
            UITableView tableView, 
            Expression<Func<TModel, RxList<TModelItem>>> modelProperty,
            Func<TModelItem, TCell> cellFactory
        )
            where TModel : IRxObject
            where TModelItem : IRxObject
            where TCell : RxTableViewCell<TModelItem>
        {
            var tableSource = new RxTableViewSource<RxList<TModelItem>, TModelItem, TCell>(tableView, x => x, (section, item) => cellFactory(item));
            var result = view
                .ObserveModelProperty(modelProperty)
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
            IRxCommand command = null;
            EventHandler listener = (sender, args) =>
            {
                if (command != null)
                    command.ExecuteAsync();
            };
            button.TouchUpInside += listener;
            var buttonDisposable = new ActionDisposable(() => button.TouchUpInside -= listener);
            var subscription = binder.ObserveModelProperty().Subscribe(x => 
            {
                command = x;
            });
            return new CompositeDisposable(subscription, buttonDisposable);
        }

        public static IDisposable ConnectButton<TModel>(
            this IRxViewObject<TModel> view,
            UIBarButtonItem button,
            Expression<Func<TModel, IRxCommand>> modelProperty
        )
            where TModel : IRxObject
        {
            return null;
        }

        public static IDisposable ConnectTextField<TModel, TModelValue>(
            this IRxViewObject<TModel> view,
            UITextField textField,
            Expression<Func<TModel, TModelValue>> modelProperty
        )
            where TModel : IRxObject
        {
            return null;
        }
    }
}

