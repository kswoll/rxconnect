using System;
using System.Collections.Generic;

namespace SexyReact
{
    public interface IRxDictionaryObservables
    {
        IObservable<KeyValuePair<object, object>> Added { get; }
        IObservable<KeyValuePair<object, object>> Set { get; }
        IObservable<KeyValuePair<object, object>> Removed { get; }        
    }

    public interface IRxDictionaryObservables<TKey, TValue>
    {
        IObservable<KeyValuePair<TKey, TValue>> Added { get; }
        IObservable<KeyValuePair<TKey, TValue>> Set { get; }
        IObservable<KeyValuePair<TKey, TValue>> Removed { get; }
    }
}
