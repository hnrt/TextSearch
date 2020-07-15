using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Controls;

namespace com.hideakin.textsearch.view
{
    internal class WorkInProgress : IDisposable
    {
        private bool disposed;
        private readonly Cursor cursorLast;
        private ContentControl cc;
        private string textFinal;
        private readonly List<Control> disabledControls;

        private WorkInProgress()
        {
            disposed = false;
            cursorLast = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;
            cc = null;
            textFinal = null;
            disabledControls = new List<Control>();
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            disposed = true;
            Mouse.OverrideCursor = cursorLast;
            if (cc != null && textFinal != null)
            {
                cc.Content = textFinal;
            }
            foreach (var c in disabledControls)
            {
                c.IsEnabled = true;
            }
        }

        public WorkInProgress SetContentControl(ContentControl cc)
        {
            this.cc = cc;
            textFinal = " ";
            return this;
        }

        public WorkInProgress SetContent(string text)
        {
            if (cc != null)
            {
                cc.Content = text;
            }
            return this;
        }

        public WorkInProgress SetContent(string format, object arg0)
        {
            return SetContent(string.Format(format, arg0));
        }

        public WorkInProgress SetContent(string format, object arg0, object arg1)
        {
            return SetContent(string.Format(format, arg0, arg1));
        }

        public WorkInProgress SetContent(string format, object arg0, object arg1, object arg2)
        {
            return SetContent(string.Format(format, arg0, arg1, arg2));
        }

        public WorkInProgress SetFinalContent(string text)
        {
            textFinal = text;
            return this;
        }

        public WorkInProgress SetFinalContent(string format, object arg0)
        {
            return SetFinalContent(string.Format(format, arg0));
        }

        public WorkInProgress SetFinalContent(string format, object arg0, object arg1)
        {
            return SetFinalContent(string.Format(format, arg0, arg1));
        }

        public WorkInProgress SetFinalContent(string format, object arg0, object arg1, object arg2)
        {
            return SetFinalContent(string.Format(format, arg0, arg1, arg2));
        }

        public WorkInProgress DisableControl(Control c)
        {
            c.IsEnabled = false;
            disabledControls.Add(c);
            return this;
        }

        public static WorkInProgress Create()
        {
            return new WorkInProgress();
        }
    }
}
