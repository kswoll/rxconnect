using System.Collections.Generic;
using System.Linq;

namespace SexyReact
{
    public struct RxListChange<T>
    {
        public IEnumerable<RxListItem<T>> Added { get; }
        public IEnumerable<RxListItem<T>> Removed { get; }
        public IEnumerable<RxListModifiedItem<T>> Modified { get; }
        public RxListMovedItem<T>? Moved { get; }

        public RxListChange(IEnumerable<RxListItem<T>> added = null, IEnumerable<RxListItem<T>> removed = null, IEnumerable<RxListModifiedItem<T>> modified = null, RxListMovedItem<T>? moved = null)
        {
            Added = added ?? Enumerable.Empty<RxListItem<T>>();
            Removed = removed ?? Enumerable.Empty<RxListItem<T>>();
            Modified = modified ?? Enumerable.Empty<RxListModifiedItem<T>>();
            Moved = moved;
        }
    }
}
