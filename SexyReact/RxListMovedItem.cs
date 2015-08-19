namespace SexyReact
{
    public struct RxListMovedItem<T>
    {
        public int FromIndex { get; }
        public int ToIndex { get; }
        public T Value { get; }

        public RxListMovedItem(int fromIndex, int toIndex, T value) : this()
        {
            FromIndex = fromIndex;
            ToIndex = toIndex;
            Value = value;
        }
    }
}
