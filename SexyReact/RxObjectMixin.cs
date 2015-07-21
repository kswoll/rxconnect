using System;
using System.Runtime.CompilerServices;

namespace SexyReact
{
    /// <summary>
    /// Use an instance of this class when you want to conveniently wrap the behavior of an RxObject but you
    /// cannot subclass it because you must subclass some other (generally UI) object instead.  On your UI object,
    /// store an instance of this class as a field, then implement IRxObject and implement all the methods by 
    /// invoking the equivalent method on this class.  When declaring the field to store this instance, declare it
    /// as an IRxObjectMixin rather than an RxObjectMixin.  This is because through the interface we can change the 
    /// accessibility of the Get/Set methods declared in RxObject.
    /// </summary>
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

