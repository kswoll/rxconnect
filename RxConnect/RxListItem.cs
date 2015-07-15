namespace RxConnect
{
    public struct RxListItem<T>
    {
        private readonly int index;
        private readonly T value;

        public RxListItem(int index, T value)
        {
            this.index = index;
            this.value = value;
        }

        public int Index
        {
            get { return index; }
        }

        public T Value
        {
            get { return value; }
        }
    }
}
