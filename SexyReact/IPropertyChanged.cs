using System.Reflection;

namespace SexyReact
{
    public interface IPropertyChanged
    {
        PropertyInfo Property { get; }
        object OldValue { get; }
        object NewValue { get; }
    }

    public interface IPropertyChanged<out T> : IPropertyChanged
    {
        new T OldValue { get; }
        new T NewValue { get; }
    }
}