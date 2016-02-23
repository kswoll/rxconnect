using System.Collections;
using System.Collections.Generic;

namespace SexyReact
{
    public interface IRxDictionary : IDictionary, IRxDictionaryObservables 
    {
    }

    public interface IRxDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IRxDictionaryObservables<TKey, TValue>
    {
    }
}
