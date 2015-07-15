using System.Reflection;

namespace RxConnect
{
    public interface IStorageStrategy
    {
        T Retrieve<T>(PropertyInfo property);
        void Store<T>(PropertyInfo property, T value);
    }
}