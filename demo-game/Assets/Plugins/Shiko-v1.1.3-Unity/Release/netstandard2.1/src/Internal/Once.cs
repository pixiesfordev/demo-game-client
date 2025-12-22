using System;
using System.Threading;

namespace Shiko.Internal.Once
{
    internal class Once
    {
        private readonly object _lock = new object();
        private Action _callback;
        private bool _hasRun;

        public Once(Action? callback)
        {
            _callback = callback ?? (() => { });
            _hasRun = false;
        }

        public void Call()
        {
            lock (_lock)
            {
                if (!_hasRun)
                {
                    _hasRun = true;
                    SafeCall(_callback);
                }
            }
        }

        public void Reset()
        {
            lock (_lock)
            {
                _hasRun = false;
            }
        }

        private static void SafeCall(Action callback)
        {
            try
            {
                if (callback == null)
                    return;

                callback?.Invoke();
            }
            catch (Exception ex)
            {
                Logger.Logger.Error($"Recovered from exception: {ex}");
            }
        }
    }
}
