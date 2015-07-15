namespace RxConnect
{
    public interface IPropertyChanged
    {
        object OldValue { get; }
        object NewValue { get; }
    }

    public interface IPropertyChanged<out T> : IPropertyChanged
    {
        new T OldValue { get; }
        new T NewValue { get; }
    }
}