using System.Collections.Generic;
using System.Reflection;

namespace SexyReact
{
    public class DictionaryStorageStrategy : IStorageStrategy
    {
        private Dictionary<string, object> values = new Dictionary<string, object>();

        public T Retrieve<T>(PropertyInfo property)
        {
            object result;
            if (values.TryGetValue(property.Name, out result))
                return (T)result;
            else
                return default(T);
        }

        public void Store<T>(PropertyInfo property, T value)
        {
            values[property.Name] = value;
        }

        public void Dispose()
        {
        }
    }
}