namespace SexyReact
{
    /// <summary>
    /// Struct to provide info on an item and its index at the time an action (such as insertion, removal) occurred.
    /// </summary>
    public struct RxListItem<T>
    {
        /// <summary>
        /// The index of the item at the time an action occurred.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// The element within the RxList that was involved in some action.
        /// </summary>
        public T Value { get; }

        public RxListItem(int index, T value)
        {
            Index = index;
            Value = value;
        }
    }
}
