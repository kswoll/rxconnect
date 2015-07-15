namespace RxConnect
{
    public struct RxListModifiedItem<T>
    {
        private int index;
        private readonly T oldValue;
        private readonly T newValue;

        public RxListModifiedItem(int index, T oldValue, T newValue)
        {
            this.index = index;
            this.oldValue = oldValue;
            this.newValue = newValue;
        }

        public int Index
        {
            get { return index; }
        }

        public T OldValue
        {
            get { return oldValue; }
        }

        public T NewValue
        {
            get { return newValue; }
        }
    }
}