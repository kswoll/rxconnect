using System;
using System.Reflection;

namespace SexyReact
{
    /// <summary>
    /// Structure to hold the old and new value when a property is about to be changed.  The new value
    /// may be re-assigned in order to affect the ultimate value of the property.
    /// </summary>
    public struct PropertyChanging<T> : IPropertyChanging<T>
    {
        public PropertyInfo Property { get; }
        public T OldValue { get; }

        private readonly Func<T> getNewValue;
        private readonly Action<T> setNewValue;

        public PropertyChanging(PropertyInfo property, T oldValue, Func<T> getNewValue, Action<T> setNewValue) : this()
        {
            Property = property;
            OldValue = oldValue;
            this.getNewValue = getNewValue;
            this.setNewValue = setNewValue;
        }

        public T NewValue
        {
            get { return getNewValue(); }
            set { setNewValue(value); }
        }

        object IPropertyChanging.OldValue => OldValue;

        object IPropertyChanging.NewValue
        {
            get { return NewValue; }
            set { NewValue = (T)value; }
        }
    }
}