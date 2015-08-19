namespace SexyReact
{
    public struct RxListModifiedItem<T>
    {
        public int Index { get; }
        public T OldValue { get; }
        public T NewValue { get; }

        public RxListModifiedItem(int index, T oldValue, T newValue)
        {
            Index = index;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}