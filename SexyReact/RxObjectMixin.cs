using System;
using System.Runtime.CompilerServices;

namespace SexyReact
{
    public class RxObjectMixin : RxObject, IRxObjectMixin
    {
        public RxObjectMixin()
        {
        }

        public RxObjectMixin(IStorageStrategy storageStrategy, IObservePropertyStrategy observePropertyStrategy) : base(storageStrategy, observePropertyStrategy)
        {
        }

        TValue IRxObjectMixin.Get<TValue>(string propertyName)
        {
            return Get<TValue>(propertyName);
        }

        void IRxObjectMixin.Set<TValue>(TValue value, string propertyName)
        {
            Set(value, propertyName);
        }
    }
}

