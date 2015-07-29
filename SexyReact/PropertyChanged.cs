using System.Reflection;

namespace SexyReact
{
    /// <summary>
    /// Structure to hold both the old value and the new value when a property has changed.
    /// </summary>
    public struct PropertyChanged<T> : IPropertyChanged<T>
    {
        public PropertyInfo Property { get; }
        public T OldValue { get; }
        public T NewValue { get; }

        public PropertyChanged(PropertyInfo property, T oldValue, T newValue)
        {
            Property = property;
            OldValue = oldValue;
            NewValue = newValue;
        }

        object IPropertyChanged.OldValue => OldValue;
        object IPropertyChanged.NewValue => NewValue;
    }
}