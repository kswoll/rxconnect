using System.Collections.Generic;
using System.Linq;

namespace SexyReact
{
    public struct RxListChange<T>
    {
        private readonly IEnumerable<RxListItem<T>> added;
        private readonly IEnumerable<RxListItem<T>> removed;
        private readonly IEnumerable<RxListMovedItem<T>> moved;
        private readonly IEnumerable<RxListModifiedItem<T>> modified;

        public RxListChange(IEnumerable<RxListItem<T>> added = null, IEnumerable<RxListItem<T>> removed = null, IEnumerable<RxListMovedItem<T>> moved = null, IEnumerable<RxListModifiedItem<T>> modified = null)
        {
            this.added = added ?? Enumerable.Empty<RxListItem<T>>();
            this.removed = removed ?? Enumerable.Empty<RxListItem<T>>();
            this.moved = moved ?? Enumerable.Empty<RxListMovedItem<T>>();
            this.modified = modified ?? Enumerable.Empty<RxListModifiedItem<T>>();
        }

        public IEnumerable<RxListItem<T>> Added
        {
            get { return added; }
        }

        public IEnumerable<RxListItem<T>> Removed
        {
            get { return removed; }
        }

        public IEnumerable<RxListMovedItem<T>> Moved
        {
            get { return moved; }
        }

        public IEnumerable<RxListModifiedItem<T>> Modified
        {
            get { return modified; }
        }
    }
}
