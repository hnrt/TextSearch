using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace com.hideakin.textsearch.view
{
    internal class TextBlockDecoration : IMultiValueConverter
    {
        public static readonly DependencyProperty InlinesProperty =
            DependencyProperty.RegisterAttached("Inlines", typeof(IEnumerable<Inline>),
            typeof(TextBlockDecoration),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnInlinesChanged)));

        public static IEnumerable<Inline> GetInlines(DependencyObject obj)
        {
            return (IEnumerable<Inline>)obj.GetValue(InlinesProperty);
        }

        public static void SetInlines(DependencyObject obj, string value)
        {
            obj.SetValue(InlinesProperty, value);
        }

        private static void OnInlinesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBlock t)
            {
                t.Inlines.Clear();
                if (e.NewValue is IEnumerable<Inline> i)
                {
                    t.Inlines.AddRange(i);
                }
            }
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length == 2 && values[0] is string text)
            {
                List<Inline> inlines = new List<Inline>();
                int index = 0;
                if (values[1] is List<(int Start, int End)> matches)
                {
                    foreach (var (start, end) in matches)
                    {
                        if (index < start)
                        {
                            inlines.Add(new Run(text.Substring(index, start - index)));
                        }
                        inlines.Add(new Run(text.Substring(start, end - start)) { Foreground = Brushes.Red });
                        index = end;
                    }
                }
                if (index < text.Length)
                {
                    inlines.Add(new Run(text.Substring(index, text.Length - index)));
                }
                return inlines;
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
