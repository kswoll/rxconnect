using System;
using System.Reflection;

namespace RxConnect
{
    public interface IStorageStrategy : IDisposable
    {
        T Retrieve<T>(PropertyInfo property);
        void Store<T>(PropertyInfo property, T value);
    }
}