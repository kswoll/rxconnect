using System.Reflection;

namespace SexyReact
{
    /// <summary>
    /// Structure to hold both the old value and the new value when a property has changed.
    /// </summary>
    public struct PropertyChanged<T> : IPropertyChanged<T>
    {
        private readonly PropertyInfo property;
        private readonly T oldValue;
        private readonly T newValue;

        public PropertyChanged(PropertyInfo property, T oldValue, T newValue)
        {
            this.property = property;
            this.oldValue = oldValue;
            this.newValue = newValue;
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
            get { return newValue; }
        }

        object IPropertyChanged.OldValue
        {
            get { return OldValue; }
        }

        object IPropertyChanged.NewValue
        {
            get { return NewValue; }
        }
    }
}