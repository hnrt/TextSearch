using System;
using System.Windows.Controls;

namespace com.hideakin.textsearch.view
{
    internal class TemporalText : IDisposable
    {
        private readonly ContentControl cc;

        private string endText;

        public TemporalText(ContentControl cc, string text, string endText = " ")
        {
            cc.Content = text;
            this.cc = cc;
            this.endText = endText;
        }

        public void Reset(string text)
        {
            cc.Content = text;
            endText = null;
        }

        public void Reset(string format, object arg0)
        {
            cc.Content = string.Format(format, arg0);
            endText = null;
        }

        public void Reset(string format, object arg0, object arg1)
        {
            cc.Content = string.Format(format, arg0, arg1);
            endText = null;
        }

        public void Reset(string format, object arg0, object arg1, object arg2)
        {
            cc.Content = string.Format(format, arg0, arg1, arg2);
            endText = null;
        }

        public void Dispose()
        {
            if (endText != null)
            {
                cc.Content = endText;
            }
        }
    }
}
