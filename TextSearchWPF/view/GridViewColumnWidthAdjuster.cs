using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace com.hideakin.textsearch.view
{
    internal class GridViewColumnWidthAdjuster
    {
        private const double LISTVIEW_COLUMN_WIDTH_ADJUSTMENT = 6.0;
        private readonly GridViewColumn targetColumn;
        private readonly GridViewColumn[] fixedColumns;
        private readonly ScrollContentPresenter container;
        private double containerWidth = double.NaN;

        public GridViewColumnWidthAdjuster(ListView host, GridViewColumn targetColumn, params GridViewColumn[] fixedColumns)
        {
            this.targetColumn = targetColumn;
            this.fixedColumns = (GridViewColumn[])fixedColumns.Clone();
            container = host.GetScrollContentPresenter();
            container.SizeChanged += OnScrollContentPresenterSizeChanged;
        }

        public void Adjust()
        {
            var w = containerWidth = container.ActualWidth;
            foreach (var f in fixedColumns)
            {
                w -= f.ActualWidth;
            }
            w -= LISTVIEW_COLUMN_WIDTH_ADJUSTMENT;
            if (w > 0)
            {
                targetColumn.Width = w;
            }
        }

        /// <summary>
        /// ScrollContentPresenter's SizeChanged event is fired even when its ActualWidth is not changed.
        /// For example, it is fired when a user double-clicked at the right edge of the header in order to fit the width automatically.
        /// To prevent such operations from being reverted, the adjustment for the target column needs to be done only when ActualWidth was changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScrollContentPresenterSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (container.ActualWidth != containerWidth)
            {
                Adjust();
            }
        }
    }
}
