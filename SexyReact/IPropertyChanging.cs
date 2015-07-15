namespace RxConnect
{
    public interface IPropertyChanging
    {
        object OldValue { get; }
        object NewValue { get; set; }
    }

    public interface IPropertyChanging<T> : IPropertyChanging
    {
        new T OldValue { get; }
        new T NewValue { get; set; }
    }
}