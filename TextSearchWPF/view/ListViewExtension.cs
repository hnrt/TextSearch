using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace com.hideakin.textsearch.view
{
    internal static class ListViewExtension
    {
        public static ScrollViewer GetScrollViewer(this ListView lv)
        {
            return lv.GetVisualChild<ScrollViewer>();
        }

        public static int GetViewportTopIndex(this ListView lv)
        {
            return (int)lv.GetScrollViewer().VerticalOffset;
        }

        public static void SetViewportTopIndex(this ListView lv, int index)
        {
            lv.GetScrollViewer().ScrollToVerticalOffset((double)index);
        }

        public static int GetViewportHeight(this ListView lv)
        {
            return (int)lv.GetScrollViewer().ViewportHeight;
        }

        public static ScrollContentPresenter GetScrollContentPresenter(this ListView lv)
        {
            var sv = lv.GetScrollViewer();
            return sv.Template.FindName("PART_ScrollContentPresenter", sv) as ScrollContentPresenter;
        }

        public static ScrollBar GetHorizontalScrollBar(this ListView lv)
        {
            var sv = lv.GetScrollViewer();
            return sv.Template.FindName("PART_HorizontalScrollBar", sv) as ScrollBar;
        }

        public static ScrollBar GetVerticalScrollBar(this ListView lv)
        {
            var sv = lv.GetScrollViewer();
            return sv.Template.FindName("PART_VerticalScrollBar", sv) as ScrollBar;
        }
    }
}
