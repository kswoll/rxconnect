using System.Collections.Generic;
using System.Reflection;

namespace RxConnect
{
    public class DictionaryStorageStrategy : IStorageStrategy
    {
        private Dictionary<PropertyInfo, object> values = new Dictionary<PropertyInfo, object>();

        public T Retrieve<T>(PropertyInfo property)
        {
            object result;
            values.TryGetValue(property, out result);
            return (T)result;
        }

        public void Store<T>(PropertyInfo property, T value)
        {
            values[property] = value;
        }
    }
}