using System.Collections.Generic;
using System.Linq;

namespace RxConnect
{
    public struct RxListChange<T>
    {
        private readonly IEnumerable<RxListItem<T>> added;
        private readonly IEnumerable<RxListItem<T>> removed;
        private readonly IEnumerable<RxListMovedItem<T>> moved;
        private readonly IEnumerable<RxListItem<T>> modified;

        public RxListChange(IEnumerable<RxListItem<T>> added = null, IEnumerable<RxListItem<T>> removed = null, IEnumerable<RxListMovedItem<T>> moved = null, IEnumerable<RxListItem<T>> modified = null)
        {
            this.added = added ?? Enumerable.Empty<RxListItem<T>>();
            this.removed = removed ?? Enumerable.Empty<RxListItem<T>>();
            this.moved = moved ?? Enumerable.Empty<RxListMovedItem<T>>();
            this.modified = modified ?? Enumerable.Empty<RxListItem<T>>();
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

        public IEnumerable<RxListItem<T>> Modified
        {
            get { return modified; }
        }
    }
}
