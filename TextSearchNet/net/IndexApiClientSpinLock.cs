using System;
using System.Threading;

namespace com.hideakin.textsearch.net
{
    internal class IndexApiClientSpinLock : IDisposable
    {
        private static int mutex = 0;

        public IndexApiClientSpinLock()
        {
            while (Interlocked.CompareExchange(ref mutex, 1, 0) == 1)
            {
                Thread.Sleep(0);
            }
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref mutex, 0);
        }
    }
}
