using System.Threading;

namespace Threads
{
    public class ThreadResult<T>
    {
        private T _value;
        private bool _hasValue;
        private AutoResetEvent _readyEvent;

        public ThreadResult()
        {
            _readyEvent = new AutoResetEvent(false);
        }

        public T GetValue()
        {
            _readyEvent.WaitOne();
            return _value;
        }

        public bool IsReady()
        {
            return _hasValue;
        }

        public void Ready(T value)
        {
            _value = value;
            _hasValue = true;
            _readyEvent.Set();
        }

        public void Reset()
        {
            _value = default;
            _hasValue = false;
        }
    }
}