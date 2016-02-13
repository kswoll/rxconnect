using System.Reflection;

namespace SexyReact
{
    public interface IPropertyChanging
    {
        PropertyInfo Property { get; }
        object OldValue { get; }
        object NewValue { get; set; }
    }

    public interface IPropertyChanging<T> : IPropertyChanging
    {
        new T OldValue { get; }
        new T NewValue { get; set; }
    }
}