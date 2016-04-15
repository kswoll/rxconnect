using System.Windows;
using System.Windows.Controls;

namespace SexyReact.Views
{
    public class RxDataGrid<T> : DataGrid
        where T : IRxObject
    {
        public double MaxPreferredHeight { get; set; }

        protected override Size MeasureOverride(Size availableSize)
        {
            var size = base.MeasureOverride(availableSize);
            if (MaxPreferredHeight > 0 && size.Height > MaxPreferredHeight)
            {
                size.Height = MaxPreferredHeight;
            }
            return size;
        }
    }
}