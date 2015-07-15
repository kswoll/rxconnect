namespace SexyReact
{
    public struct RxListMovedItem<T>
    {
        private readonly int fromIndex;
        private readonly int toIndex;
        private readonly T value;

        public RxListMovedItem(int fromIndex, int toIndex, T value) : this()
        {
            this.fromIndex = fromIndex;
            this.toIndex = toIndex;
            this.value = value;
        }

        public int FromIndex
        {
            get { return fromIndex; }
        }

        public int ToIndex
        {
            get { return toIndex; }
        }

        public T Value
        {
            get { return value; }
        }
    }
}
