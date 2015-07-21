namespace SexyReact
{
    /// <summary>
    /// Struct to provide info on an item and its index at the time an action (such as insertion, removal) occurred.
    /// </summary>
    public struct RxListItem<T>
    {
        private readonly int index;
        private readonly T value;

        public RxListItem(int index, T value)
        {
            this.index = index;
            this.value = value;
        }

        /// <summary>
        /// The index of the item at the time an action occurred.
        /// </summary>
        public int Index
        {
            get { return index; }
        }

        /// <summary>
        /// The element within the RxList that was involved in some action.
        /// </summary>
        public T Value
        {
            get { return value; }
        }
    }
}
