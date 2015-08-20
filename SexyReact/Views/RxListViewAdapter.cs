using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using SexyReact.Utils;
using System.Linq;
using System.Collections.ObjectModel;

namespace SexyReact.Views
{
    public enum RxListViewAdapterCachingPolicy
    {
        Reuse, Cache, GlobalCache
    }

    public class RxListViewAdapter<TView, TSection, TItem, TCell> 
        where TItem : IRxObject
        where TCell : IRxViewObject<TItem>
    {
        private TView view;
        private IRxList<TSection> data;
        private Func<TSection, IRxList<TItem>> itemsInSection;
        private Dictionary<TItem, TCell> cellsByItem = new Dictionary<TItem, TCell>();
        private List<TCell> globalCellCache = new List<TCell>();
        private Func<TSection, TItem, TCell> cellFactory;
        private IDisposable sectionAddedSubscription;
        private IDisposable sectionRemovedSubscription;
        private Dictionary<TSection, IDisposable[]> sectionSubscriptions = new Dictionary<TSection, IDisposable[]>();
        private List<Tuple<TSection, ObservableCollection<TItem>>> localCopy = new List<Tuple<TSection, ObservableCollection<TItem>>>();
        private RxListViewAdapterCachingPolicy cachingPolicy = RxListViewAdapterCachingPolicy.GlobalCache;
        private bool disposed;
        private Func<TCell> reuseCellProvider;

        private Action<int> sectionAdded;
        private Action<int> sectionRemoved;
        private Action<int, TSection, IEnumerable<Tuple<int, TItem>>, Action> itemsAdded;
        private Action<int, TSection, IEnumerable<Tuple<int, TItem>>, Action> itemsRemoved;

        public RxListViewAdapter(
            TView view, 
            Func<TSection, IRxList<TItem>> itemsInSection,
            Func<TSection, TItem, TCell> cellFactory,
            Action<int> sectionAdded,
            Action<int> sectionRemoved,
            Action<int, TSection, IEnumerable<Tuple<int, TItem>>, Action> itemsAdded,
            Action<int, TSection, IEnumerable<Tuple<int, TItem>>, Action> itemsRemoved,
            Func<TCell> reuseCellProvider
        )
        {
            this.view = view;
            this.itemsInSection = itemsInSection;
            this.cellFactory = cellFactory;
            this.sectionAdded = sectionAdded;
            this.sectionRemoved = sectionRemoved;
            this.itemsAdded = itemsAdded;
            this.itemsRemoved = itemsRemoved;
            this.reuseCellProvider = reuseCellProvider;
        }

        public TView View
        {
            get { return view; }
        }

        public IRxList<TItem> GetItems(int sectionIndex) 
        {
            var section = localCopy[sectionIndex];
            var items = itemsInSection(section.Item1);
            return items;
        }

        public void RemoveItem(int sectionIndex, int itemIndex)
        {
            var section = localCopy[sectionIndex];
            var item = section.Item2[itemIndex];
            var items = itemsInSection(section.Item1);
            items.Remove(item);
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                Dispose(true);
            }
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (data != null)
                {
                    sectionAddedSubscription.Dispose();
                    sectionRemovedSubscription.Dispose();
                }
            }
        }

        public IRxList<TSection> Data
        {
            get { return data; }
            set 
            { 
                if (data != value) 
                {
                    if (data != null)
                    {
                        sectionAddedSubscription.Dispose();
                        sectionRemovedSubscription.Dispose();
                        foreach (var section in data)
                        {
                            OnSectionRemoved(section);
                        }
                    }
                    data = value;
                    if (value != null)
                    {
                        sectionAddedSubscription = value.ItemAdded.Subscribe(OnSectionAdded);
                        sectionRemovedSubscription = value.ItemRemoved.Subscribe(OnSectionRemoved);
                        foreach (var section in value)
                        {
                            OnSectionAdded(section);
                        }
                    }
                }
            }
        }

        protected virtual void OnSectionAdded(TSection section)
        {
            var items = itemsInSection(section);
            var localItems = new ObservableCollection<TItem>();
            var sectionIndex = data.IndexOf(section);
            localCopy.Insert(sectionIndex, Tuple.Create(section, localItems));
            sectionAdded?.Invoke(data.IndexOf(section));
            var subscriptions = new[]
            {
                items.Changed.Where(x => x.Added.Any() || x.Removed.Any()).Subscribe(_ =>
                {
                    var mergeResult = localItems.Merge(items);
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
            if (cachingPolicy == RxListViewAdapterCachingPolicy.GlobalCache)
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
                foreach (var item in items)
                {
                    ReleaseCell(item);
                }
                items.Clear();
            }

            localCopy.RemoveAt(sectionIndex);
            sectionRemoved?.Invoke(sectionIndex);

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
            var sectionIndex = localCopy.IndexOf(x => Equals(x.Item1, section));
            itemsAdded?.Invoke(sectionIndex, section, items, () => 
            {
                foreach (var item in items.OrderBy(x => x.Item1))
                {
                    localCopy[sectionIndex].Item2.Insert(item.Item1, item.Item2);
                }
            });
        }

        protected virtual void OnItemsRemoved(TSection section, IEnumerable<Tuple<int, TItem>> items)
        {
            var sectionIndex = localCopy.IndexOf(x => Equals(x.Item1, section));
            var localItems = localCopy[sectionIndex];
            itemsRemoved?.Invoke(sectionIndex, section, items, () =>
            {
                foreach (var item in items.OrderByDescending(x => x.Item1))
                {
                    localItems.Item2.RemoveAt(item.Item1);
                }
            });
            foreach (var item in items)
            {
                ReleaseCell(item.Item2);
            }
        }

        public int SectionCount => localCopy.Count;

        public int RowsInSection(int sectionIndex)
        {
            var section = localCopy[sectionIndex];
            return section.Item2.Count;
        }

        public TCell GetCell(int sectionIndex, int rowIndex)
        {
            var section = localCopy[sectionIndex];
            var item = section.Item2[rowIndex];
            TCell cell;
            if (cachingPolicy == RxListViewAdapterCachingPolicy.Reuse)
            {
                cell = reuseCellProvider();
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

        public void MoveRow(int sourceSectionIndex, int sourceRowIndex, int destinationSectionIndex, int destinationRowIndex)
        {
            var sourceSection = localCopy[sourceSectionIndex];
            var destinationSection = localCopy[destinationSectionIndex];
            var item = sourceSection.Item2[sourceRowIndex];
            var sourceItems = itemsInSection(sourceSection.Item1);
            var destinationItems = itemsInSection(destinationSection.Item1);
            if (sourceSectionIndex == destinationSectionIndex)
            {
                sourceItems.Move(sourceRowIndex, destinationRowIndex);
            }
            else
            {
                sourceItems.Remove(item);
                destinationItems.Insert(destinationRowIndex, item);
            }
        }
    }
}