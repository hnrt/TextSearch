using System;
using System.Windows.Input;

namespace com.hideakin.textsearch.view
{
    internal class WaitCursor : IDisposable
    {
        private Cursor previous;

        public WaitCursor()
        {
            previous = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;
        }

        public void Dispose()
        {
            Mouse.OverrideCursor = previous;
        }
    }
}
