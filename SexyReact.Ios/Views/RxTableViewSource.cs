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
    public enum RxTableViewCellCachingPolicy
    {
        Reuse, Cache, GlobalCache
    }

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
        private Dictionary<TItem, TCell> cellsByItem = new Dictionary<TItem, TCell>();
        private List<TCell> globalCellCache = new List<TCell>();
        private Func<TSection, TItem, TCell> cellFactory;
        private IDisposable sectionAdded;
        private IDisposable sectionRemoved;
        private IDisposable itemsAdded;
        private IDisposable itemsRemoved;
        private Dictionary<TSection, IDisposable[]> sectionSubscriptions = new Dictionary<TSection, IDisposable[]>();
        private List<Tuple<TSection, List<TItem>>> localCopy = new List<Tuple<TSection, List<TItem>>>();
        private NSString cellKey;
        private RxTableViewCellCachingPolicy cachingPolicy = RxTableViewCellCachingPolicy.GlobalCache;

        public RxTableViewSource(
            UITableView tableView, 
            Func<TSection, RxList<TItem>> itemsInSection,
            Func<TSection, TItem, TCell> cellFactory)
        {
            this.tableView = tableView;
            this.itemsInSection = itemsInSection;
            this.cellFactory = cellFactory;

            cellKey = new NSString(typeof(TItem).FullName);
        }

        public UITableView TableView
        {
            get { return tableView; }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                cellKey.Dispose();
                if (data != null)
                {
                    sectionAdded.Dispose();
                    sectionRemoved.Dispose();
                    if (itemsAdded != null)
                        itemsAdded.Dispose();
                    if (itemsRemoved != null)
                        itemsRemoved.Dispose();
                }
            }
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
                    if (itemsAdded != null)
                        itemsAdded.Dispose();
                    if (itemsRemoved != null)
                        itemsRemoved.Dispose();
                    foreach (var section in data)
                    {
                        OnSectionRemoved(section);
                    }
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

        private void ReleaseCell(TItem item)
        {
            var cell = cellsByItem[item];
            cellsByItem.Remove(item);
            if (cachingPolicy == RxTableViewCellCachingPolicy.GlobalCache)
            {
                globalCellCache.Add(cell);
                cell.Model = default(TItem);
            }
        }

        protected virtual void OnSectionRemoved(TSection section)
        {
            var sectionIndex = localCopy.IndexOf(x => Equals(x.Item1, section));
            var items = localCopy[sectionIndex].Item2;
            if (items.Any())
            {
                var localItems = localCopy[sectionIndex];
                foreach (var item in items)
                {
                    ReleaseCell(item);
                }
                items.Clear();
            }

            localCopy.RemoveAt(sectionIndex);
            tableView.DeleteSections(NSIndexSet.FromIndex(data.IndexOf(section)), UITableViewRowAnimation.Automatic);

            IDisposable[] subscriptions;
            if (sectionSubscriptions.TryGetValue(section, out subscriptions))
            {
                foreach (var subscription in subscriptions)
                {
                    subscription.Dispose();
                }
            }
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
                ReleaseCell(item.Item2);
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
            TCell cell;
            if (cachingPolicy == RxTableViewCellCachingPolicy.Reuse)
            {
                cell = (TCell)tableView.DequeueReusableCell(cellKey);
                ((IRxViewObject)cell).Model = item;
            }
            else
            {
                if (!cellsByItem.TryGetValue(item, out cell))
                {
                    if (globalCellCache.Count > 0)
                    {
                        cell = globalCellCache[globalCellCache.Count - 1];
                        globalCellCache.RemoveAt(globalCellCache.Count - 1);
                    }
                    else 
                    {
                        cell = cellFactory(section.Item1, item);
                    }
                    cellsByItem[item] = cell;
                    ((IRxViewObject)cell).Model = item;
                }
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