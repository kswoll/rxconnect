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

namespace SexyReact.Ios
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
        private UITableView tableView;
        private RxList<TSection> data;
        private Func<TSection, RxList<TItem>> itemsInSection;
        private Dictionary<Tuple<int, int>, UITableViewCell> cellsByIndexPath = new Dictionary<Tuple<int, int>, UITableViewCell>();
        private Func<TSection, TItem, UITableViewCell> cellFactory;
        private IDisposable sectionAdded;
        private IDisposable sectionRemoved;
        private IDisposable itemsAdded;
        private IDisposable itemsRemoved;
        private Dictionary<TSection, IDisposable[]> sectionSubscriptions = new Dictionary<TSection, IDisposable[]>();
        private List<Tuple<TSection, List<TItem>>> localCopy = new List<Tuple<TSection, List<TItem>>>();

        public RxTableViewSource(
            UITableView tableView, 
            Func<TSection, RxList<TItem>> itemsInSection,
            Func<TSection, TItem, UITableViewCell> cellFactory)
        {
            this.tableView = tableView;
            this.itemsInSection = itemsInSection;
            this.cellFactory = cellFactory;
        }

        public UITableView TableView
        {
            get { return tableView; }
        }

        public RxList<TSection> Data
        {
            get { return data; }
            set 
            { 
                if (data != null)
                {
                    sectionAdded.Dispose();
                    sectionRemoved.Dispose();
                    itemsAdded.Dispose();
                    itemsRemoved.Dispose();
                }
                data = value;
                if (value != null)
                {
                    sectionAdded = value.ItemAdded.Subscribe(OnSectionAdded);
                    sectionRemoved = value.ItemRemoved.Subscribe(OnSectionRemoved);
                    foreach (var section in value)
                    {
                        OnSectionAdded(section);
                    }
                }
            }
        }

        protected virtual void OnSectionAdded(TSection section)
        {
            var items = itemsInSection(section);
            var localItems = new List<TItem>();
            var sectionIndex = data.IndexOf(section);
            localCopy.Insert(sectionIndex, Tuple.Create(section, localItems));
            tableView.InsertSections(NSIndexSet.FromIndex(data.IndexOf(section)), UITableViewRowAnimation.Automatic);
            var subscriptions = new[]
            {
                items.Changed.Where(x => x.Added.Any() || x.Removed.Any()).Subscribe(_ =>
                {
                    var mergeResult = localItems.Merge<TItem>(items);
                    var localItemIndices = localItems.Select((x, i) => new { Item = x, Index = i }).ToDictionary(x => x.Item, x => x.Index);
                    var itemIndices = items.Select((x, i) => new { Item = x, Index = i }).ToDictionary(x => x.Item, x => x.Index);
                    if (mergeResult.Added.Any())
                        OnItemsAdded(section, mergeResult.Added.Select(x => Tuple.Create(itemIndices[x], x)));
                    if (mergeResult.Removed.Any())
                        OnItemsRemoved(section, mergeResult.Removed.Select(x => Tuple.Create(localItemIndices[x], x)));
                })
            };
            sectionSubscriptions[section] = subscriptions;

            if (items.Any())
                OnItemsAdded(section, items.Select((x, i) => Tuple.Create(i, x)));
        }

        protected virtual void OnSectionRemoved(TSection section)
        {
            var sectionIndex = localCopy.IndexOf(x => Equals(x.Item1, section));
            var items = localCopy[sectionIndex].Item2;
            localCopy.RemoveAt(sectionIndex);

            IDisposable[] subscriptions;
            if (sectionSubscriptions.TryGetValue(section, out subscriptions))
            {
                foreach (var subscription in subscriptions)
                {
                    subscription.Dispose();
                }
            }

            if (items.Any())
                OnItemsRemoved(section, items.Select((x, i) => Tuple.Create(i, x)));
        }

        protected virtual void OnItemsAdded(TSection section, IEnumerable<Tuple<int, TItem>> items)
        {
            CATransaction.DisableActions = true;
            tableView.BeginUpdates();
            var sectionIndex = localCopy.IndexOf(x => Equals(x.Item1, section));
            var localItems = localCopy[sectionIndex];
            foreach (var item in items.OrderBy(x => x.Item1))
            {
                localCopy[sectionIndex].Item2.Insert(item.Item1, item.Item2);
            }
            tableView.InsertRows(items.Select(x => NSIndexPath.FromItemSection(x.Item1, sectionIndex)).ToArray(), 
                UITableViewRowAnimation.None);
            tableView.EndUpdates();
            CATransaction.DisableActions = false;
        }

        protected virtual void OnItemsRemoved(TSection section, IEnumerable<Tuple<int, TItem>> items)
        {
            CATransaction.DisableActions = true;
            tableView.BeginUpdates();
            var sectionIndex = localCopy.IndexOf(x => Equals(x.Item1, section));
            var localItems = localCopy[sectionIndex];
            foreach (var item in items.OrderByDescending(x => x.Item1))
            {
                localItems.Item2.RemoveAt(item.Item1);
                cellsByIndexPath.Remove(Tuple.Create(sectionIndex, item.Item1));
            }
            tableView.DeleteRows(items.Select(x => NSIndexPath.FromItemSection(x.Item1, sectionIndex)).ToArray(),
                UITableViewRowAnimation.None);
            tableView.EndUpdates();
            CATransaction.DisableActions = false;
        }

        public override nint NumberOfSections(UITableView tableView)
        {
            return localCopy.Count;
        }

        public override nint RowsInSection(UITableView tableview, nint sectionIndex)
        {
            var section = localCopy[(int)sectionIndex];
            return section.Item2.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var section = localCopy[indexPath.Section];
            var item = section.Item2[indexPath.Row];
            var key = Tuple.Create(indexPath.Section, indexPath.Row);

            UITableViewCell cell;
            if (!cellsByIndexPath.TryGetValue(key, out cell))
            {
                cell = cellFactory(section.Item1, item);
                cellsByIndexPath[key] = cell;
                ((IRxViewObject)cell).Model = item;
            }
            return cell;
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

        public override void MoveRow(UITableView tableView, NSIndexPath sourceIndexPath, NSIndexPath destinationIndexPath)
        {
            var section = localCopy[sourceIndexPath.Section];
            var item = section.Item2[sourceIndexPath.Row];
            var items = itemsInSection(section.Item1);
            items.Move(items.IndexOf(item), destinationIndexPath.Row);
        }

        public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return UITableViewCellEditingStyle.Delete;
        }

        public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
        {
            var section = localCopy[indexPath.Section];
            var item = section.Item2[indexPath.Row];
            var items = itemsInSection(section.Item1);
            switch (editingStyle)
            {
                case UITableViewCellEditingStyle.Delete:

                    items.Remove(item);
                    break;
            }
        }
    }
}