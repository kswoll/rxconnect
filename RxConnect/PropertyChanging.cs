using System;
using System.Reflection;

namespace RxConnect
{
    /// <summary>
    /// Structure to hold the old and new value when a property is about to be changed.  The new value
    /// may be re-assigned in order to affect the ultimate value of the property.
    /// </summary>
    public struct PropertyChanging<T> : IPropertyChanging<T>
    {
        private readonly PropertyInfo property;
        private readonly T oldValue;
        private readonly Func<T> getNewValue;
        private readonly Action<T> setNewValue;

        public PropertyChanging(PropertyInfo property, T oldValue, Func<T> getNewValue, Action<T> setNewValue) : this()
        {
            this.property = property;
            this.oldValue = oldValue;
            this.getNewValue = getNewValue;
            this.setNewValue = setNewValue;
        }

        public PropertyInfo Property
        {
            get { return property; }
        }

        public T OldValue
        {
            get { return oldValue; }
        }

        public T NewValue
        {
            get { return getNewValue(); }
            set { setNewValue(value); }
        }

        object IPropertyChanging.OldValue
        {
            get { return OldValue; }
        }

        object IPropertyChanging.NewValue
        {
            get { return NewValue; }
            set { NewValue = (T)value; }
        }
    }
}