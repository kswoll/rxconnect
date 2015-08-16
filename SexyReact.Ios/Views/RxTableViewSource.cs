using System;
using UIKit;
using SexyReact;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Linq;
using Foundation;
using CoreAnimation;
using SexyReact.Utils;
using SexyReact.Views;
using CoreGraphics;
using CoreFoundation;

namespace SexyReact.Views
{
    /// <summary>
    /// A table view source that works with RX lists.  The underlying data structure expects an RxList<RxList<T>>, but there are 
    /// facilities for easily working with just RxList<T>.  While you may create an instance of this class on your own, it is easier
    /// to use the Connect extension method defined in IosViewObjectExtensions.  Usage would be (in your UIViewController):
    /// 
    /// this.Connect(TableView, vm => vm.Items, () => new ItemCell());
    /// </summary>
    public class RxTableViewSource<TSection, TItem, TCell> : UITableViewSource
        where TItem : IRxObject
        where TCell : RxTableViewCell<TItem>
    {
        private RxListViewAdapter<UITableView, TSection, TItem, TCell> adapter;
        private NSString cellKey;

        public RxTableViewSource(
            UITableView tableView, 
            Func<TSection, IRxList<TItem>> itemsInSection,
            Func<TSection, TItem, TCell> cellFactory)
        {
            cellKey = new NSString(typeof(TItem).FullName);
            adapter = new RxListViewAdapter<UITableView, TSection, TItem, TCell>(tableView, itemsInSection, cellFactory, 
                sectionIndex => tableView.InsertSections(NSIndexSet.FromIndex(sectionIndex), UITableViewRowAnimation.Automatic),
                sectionIndex => tableView.DeleteSections(NSIndexSet.FromIndex(sectionIndex), UITableViewRowAnimation.Automatic),
                OnItemsAdded,
                OnItemsRemoved,
                () => (TCell)tableView.DequeueReusableCell(cellKey));
        }

        public UITableView TableView
        {
            get { return adapter.View; }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                cellKey.Dispose();
                if (adapter != null)
                {
                    adapter.Dispose();
                }
            }
        }

        public IRxList<TSection> Data
        {
            get { return adapter.Data; }
            set { adapter.Data = value; }
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            return adapter.GetCell(indexPath.Section, indexPath.Row);
        }

        private void OnItemsAdded(int sectionIndex, TSection section, IEnumerable<Tuple<int, TItem>> items)
        {
            CATransaction.DisableActions = true;
            TableView.BeginUpdates();
            TableView.InsertRows(items.Select(x => NSIndexPath.FromItemSection(x.Item1, sectionIndex)).ToArray(), 
                UITableViewRowAnimation.None);
            TableView.EndUpdates();
            CATransaction.DisableActions = false;
        }

        protected virtual void OnItemsRemoved(int sectionIndex, TSection section, IEnumerable<Tuple<int, TItem>> items)
        {
            CATransaction.DisableActions = true;
            TableView.BeginUpdates();
            TableView.DeleteRows(items.Select(x => NSIndexPath.FromItemSection(x.Item1, sectionIndex)).ToArray(),
                UITableViewRowAnimation.None);
            TableView.EndUpdates();
            CATransaction.DisableActions = false;
        }

        public override nint NumberOfSections(UITableView tableView)
        {
            return adapter.SectionCount;
        }

        public override nint RowsInSection(UITableView tableview, nint sectionIndex)
        {
            return adapter.RowsInSection((int)sectionIndex);
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = GetCell(tableView, indexPath);
            return cell.SizeThatFits(new CGSize(float.MaxValue, float.MaxValue)).Height;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = (RxTableViewCell<TItem>)GetCell(tableView, indexPath);
            if (cell.Command != null)
                cell.Command.ExecuteAsync();
        }

        public override bool CanMoveRow(UITableView tableView, NSIndexPath indexPath)
        {
            return true;
        }

        public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
        {
            return true;
        }

        public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return UITableViewCellEditingStyle.Delete;
        }

        public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
        {
            switch (editingStyle)
            {
                case UITableViewCellEditingStyle.Delete:
                    adapter.RemoveItem(indexPath.Section, indexPath.Row);
                    break;
            }
        }
    }
}