using System;
using System.Windows.Threading;

namespace com.hideakin.textsearch.view
{
    internal class Delay<T> : IDisposable
    {
        private readonly Action<T> _action;
        private object _mutex = new object();
        private DispatcherTimer _timer = null;
        private T _argument;

        public bool IsScheduled
        {
            get
            {
                lock(_mutex)
                {
                    return _timer != null;
                }
            }
        }

        public Delay(Action<T> action)
        {
            _action = action;
        }

        public void Dispose()
        {
            lock (_mutex)
            {
                if (_timer != null)
                {
                    Stop();
                }
            }
        }

        public void RunImmediately(T argument)
        {
            lock (_mutex)
            {
                _action.Invoke(argument);
                if (_timer != null)
                {
                    Stop();
                }
            }
        }

        public void Schedule(TimeSpan duration, T argument)
        {
            lock(_mutex)
            {
                if (_timer != null)
                {
                    Stop();
                }
                _argument = argument;
                _timer = new DispatcherTimer();
                _timer.Interval = duration;
                _timer.Tick += Callback;
                _timer.Start();
            }
        }

        private void Stop()
        {
            _timer.Stop();
            _timer.Tick -= Callback;
            _timer = null;
        }

        private void Callback(object sender, EventArgs e)
        {
            lock (_mutex)
            {
                if (_timer != null)
                {
                    _action.Invoke(_argument);
                    Stop();
                }
            }
        }
    }
}
