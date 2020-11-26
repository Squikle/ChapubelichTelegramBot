using System;
using System.Threading;

namespace ChapubelichBot.Types.Managers.MessagesSender.Limiters
{
    class GlobalLimiter : IDisposable
    {
        private readonly int _globalLimitMessagesPerInterval;

#pragma warning disable IDE0052, IDE0044
        private static object _locker = new object();
        private Timer _timer;
#pragma warning restore IDE0052, IDE0044

        private int _messagesCounter;
        private bool _isAllowed;

        public GlobalLimiter(int globalLimitMessagesPerInterval, int globalLimitMessagesDelay)
        {
            _globalLimitMessagesPerInterval = globalLimitMessagesPerInterval;
            int delay = globalLimitMessagesDelay <= 0 ? 100 : globalLimitMessagesDelay;

            _timer = new Timer(t =>
            {
                lock (_locker)
                {
                    _isAllowed = true;
                    _messagesCounter = 0;
                }
            }, null, 0, delay);
        }

        public void RequestCreated()
        {
            lock (_locker)
            {
                _messagesCounter++;
                if (_messagesCounter >= _globalLimitMessagesPerInterval)
                    _isAllowed = false;
            }
        }

        public bool IsAvailableToSend()
        {
            return _isAllowed;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
