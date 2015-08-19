using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SexyReact.Utils
{
    public static class Enumerables
    {
        public static IEnumerable<T> Return<T>(T value)
        {
            return new ReturnEnumerable<T>(value);
        }

        private struct ReturnEnumerable<T> : IEnumerable<T>
        {
            private readonly T value;

            public ReturnEnumerable(T value) : this()
            {
                this.value = value;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IEnumerator<T> GetEnumerator()
            {
                return new ReturnEnumerator<T>(value);
            }
        }

        private struct ReturnEnumerator<T> : IEnumerator<T>
        {
            public T Current { get; }

            private bool hasReadValue;

            public ReturnEnumerator(T value) : this()
            {
                Current = value;
            }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (!hasReadValue)
                {
                    hasReadValue = true;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void Reset()
            {
                hasReadValue = false;
            }
        }

        /// <summary>
        /// Compares `source` with `mergeWith`.  Items that are contained in `mergeWith` that are not contained in `source` 
        /// are placed in the `Added` property on the return value.  Items that are contained in `source` but not contained
        /// in `mergeWith` are placed in the `Removed` property on the return value.
        /// </summary>
        public static MergeResult<T, T, T> Merge<T>(this IEnumerable<T> source, IEnumerable<T> mergeWith)
        {
            return source.Merge(mergeWith, x => x, x => x, (x, y) => x);
        }

        public static MergeResult<TLeft, TRight, Tuple<TLeft, TRight>> Merge<TLeft, TRight, TKey>(this IEnumerable<TLeft> source, IEnumerable<TRight> mergeWith, Func<TLeft, TKey> leftKeySelector, Func<TRight, TKey> rightKeySelector)
        {
            return source.Merge(mergeWith, leftKeySelector, rightKeySelector, (x, y) => Tuple.Create(x, y));
        }

        public static MergeResult<TLeft, TRight, TUnchanged> Merge<TLeft, TRight, TUnchanged, TKey>(this IEnumerable<TLeft> source, IEnumerable<TRight> mergeWith, Func<TLeft, TKey> leftKeySelector, Func<TRight, TKey> rightKeySelector, Func<TLeft, TRight, TUnchanged> unchangedSelector)
        {
            Dictionary<TKey, TLeft> sourceSet = source.ToDictionary(x => leftKeySelector(x));
            var mergeWithKeys = mergeWith
                .Select(x => new { Item = x, Key = rightKeySelector(x) })
                .ToArray();
            Dictionary<TKey, TRight> mergeWithById;
            try
            {
                mergeWithById = mergeWithKeys
                    .Where(x => !EqualityComparer<TKey>.Default.Equals(x.Key, default(TKey)))
                    .ToDictionary(x => x.Key, x => x.Item);
            }
            catch (Exception)
            {
                var dups = mergeWithKeys.GroupBy(x => x.Key).Where(x => x.Count() > 1).ToArray();
                if (dups.Any())
                {
                    throw new Exception("Duplicate keys found: " + string.Join(", ", dups.Select(x => x.Key)));
                }
                else
                {
                    throw;
                }
            }
            TRight[] newMergeWith = mergeWithKeys.Where(x => EqualityComparer<TKey>.Default.Equals(x.Key, default(TKey))).Select(x => x.Item).ToArray();

            List<TLeft> removed = sourceSet.Where(item => !mergeWithById.Select(x => x.Key).Contains(item.Key)).Select(x => x.Value).ToList();
            List<TRight> added = newMergeWith.Concat(mergeWithById.Where(item => !sourceSet.Select(x => x.Key).Contains(item.Key)).Select(x => x.Value)).ToList();
            TKey[] commonKeys = sourceSet.Select(x => x.Key).Intersect(mergeWithById.Select(x => x.Key)).ToArray();
            List<TUnchanged> unchanged = commonKeys.Select(x => unchangedSelector(sourceSet[x], mergeWithById[x])).ToList();

            return new MergeResult<TLeft, TRight, TUnchanged>(added, removed, unchanged);
        }

        public struct MergeResult<TLeft, TRight, TUnchanged>
        {
            public List<TRight> Added { get; }
            public List<TLeft> Removed { get; }
            public List<TUnchanged> Unchanged { get; }

            public MergeResult(List<TRight> added, List<TLeft> removed, List<TUnchanged> unchanged) : this()
            {
                Added = added;
                Removed = removed;
                Unchanged = unchanged;
            }
        }

        public static int IndexOf<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            var index = 0;
            foreach (var item in source)
            {
                if (predicate(item))
                    return index;
                index++;
            }
            return -1;
        }
    }
}
