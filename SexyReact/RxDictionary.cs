using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace SexyReact
{
    public class RxDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IRxDictionaryObservables<TKey, TValue>, IRxDictionaryObservables, IDictionary
    {
        private Dictionary<TKey, TValue> storage = new Dictionary<TKey, TValue>();
        private Lazy<Subject<KeyValuePair<TKey, TValue>>> added = new Lazy<Subject<KeyValuePair<TKey, TValue>>>();
        private Lazy<Subject<KeyValuePair<TKey, TValue>>> set = new Lazy<Subject<KeyValuePair<TKey, TValue>>>();
        private Lazy<Subject<KeyValuePair<TKey, TValue>>> removed = new Lazy<Subject<KeyValuePair<TKey, TValue>>>();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return storage.GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            storage.Add(item.Key, item.Value);

        }

        public void Clear()
        {
            foreach (var key in storage.Keys.ToArray())
            {
                Remove(key);
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) => ((IDictionary<TKey, TValue>)storage).Contains(item);
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((IDictionary<TKey, TValue>)storage).CopyTo(array, arrayIndex);
        public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);
        public int Count => storage.Count;
        public bool IsReadOnly => false;
        public bool ContainsKey(TKey key) => storage.ContainsKey(key);

        public void Add(TKey key, TValue value)
        {
            storage.Add(key, value);
            if (added.IsValueCreated)
            {
                added.Value.OnNext(new KeyValuePair<TKey, TValue>(key, value));
            }
        }

        public bool Remove(TKey key)
        {
            TValue value;
            if (storage.TryGetValue(key, out value))
            {
                storage.Remove(key);
                if (removed.IsValueCreated)
                    removed.Value.OnNext(new KeyValuePair<TKey, TValue>(key, value));
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return storage.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get { return storage[key]; }
            set
            {
                TValue _;
                if (storage.TryGetValue(key, out _))
                {
                    storage[key] = value;
                }
                else
                {
                    storage[key] = value;
                    if (added.IsValueCreated)
                        added.Value.OnNext(new KeyValuePair<TKey, TValue>(key, value));
                }
                if (set.IsValueCreated)
                    set.Value.OnNext(new KeyValuePair<TKey, TValue>(key, value));
            }
        }

        public ICollection<TKey> Keys => storage.Keys;
        public ICollection<TValue> Values => storage.Values;

        public IObservable<KeyValuePair<TKey, TValue>> Added => added.Value;
        public IObservable<KeyValuePair<TKey, TValue>> Set => set.Value;
        public IObservable<KeyValuePair<TKey, TValue>> Removed => removed.Value;

        IObservable<KeyValuePair<object, object>> IRxDictionaryObservables.Added => Added.Select(x => new KeyValuePair<object, object>(x.Key, x.Value));
        IObservable<KeyValuePair<object, object>> IRxDictionaryObservables.Set => Set.Select(x => new KeyValuePair<object, object>(x.Key, x.Value));
        IObservable<KeyValuePair<object, object>> IRxDictionaryObservables.Removed => Removed.Select(x => new KeyValuePair<object, object>(x.Key, x.Value));

        void ICollection.CopyTo(Array array, int index) => ((IDictionary)storage).CopyTo(array, index);
        object ICollection.SyncRoot => ((IDictionary)storage).SyncRoot;
        bool ICollection.IsSynchronized => ((IDictionary)storage).IsSynchronized;
        bool IDictionary.Contains(object key) => ((IDictionary)storage).Contains(key);
        void IDictionary.Add(object key, object value) => Add((TKey)key, (TValue)value);
        IDictionaryEnumerator IDictionary.GetEnumerator() => ((IDictionary)storage).GetEnumerator();
        void IDictionary.Remove(object key) => Remove((TKey)key);
        ICollection IDictionary.Keys => ((IDictionary)storage).Keys;
        ICollection IDictionary.Values => ((IDictionary)storage).Values;
        bool IDictionary.IsFixedSize => false;

        object IDictionary.this[object key]
        {
            get { return this[(TKey)key]; }
            set { this[(TKey)key] = (TValue)value; }
        }
    }
}
