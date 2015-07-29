using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SexyReact
{
    /// <summary>
    /// Use RxListExtensions.Derive instead.
    /// </summary>
    public class RxFilteredList<TSource, T, TValue> : RxDerivedList<TSource, T>
        where TSource : IRxObject
    {
        private IRxList<TSource> source;
        private List<int> destinationIndicesBySourceIndex;
        private HashSet<TSource> set;

        protected internal RxFilteredList(IRxList<TSource> source, Func<TSource, T> selector, Expression<Func<TSource, TValue>> filterSource,
            Func<TValue, bool> filter, Action<T> removed = null) : base(source, selector, removed)
        {
            destinationIndicesBySourceIndex = new List<int>();
            set = new HashSet<TSource>();

            ReIndex();
        }

        protected override void OnSourceChanged(RxListChange<TSource> changes)
        {
            base.OnSourceChanged(changes);

//            foreach (var item in changes.Added)
//            {
//                set.Add()
//            }

            ReIndex();
        }

        private void ReIndex()
        {
            destinationIndicesBySourceIndex.Clear();
            var destinationIndex = 0;
            for (var sourceIndex = 0; sourceIndex < source.Count; sourceIndex++)
            {
                var sourceItem = source[sourceIndex];

//                if (set.Contains())
            }
        }
    }
}
