using System;

namespace com.hideakin.textsearch.view
{
    internal class DisableControl : IDisposable
    {
        private System.Windows.Controls.Control target;

        public DisableControl(System.Windows.Controls.Control target)
        {
            this.target = target;
            this.target.IsEnabled = false;
        }

        public void Dispose()
        {
            target.IsEnabled = true;
        }
    }
}
