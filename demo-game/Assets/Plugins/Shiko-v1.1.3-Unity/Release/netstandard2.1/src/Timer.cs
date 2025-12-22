using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Shiko.Internal.Logger;

namespace Shiko.Timer
{
    public interface ITimer
    {
        void Check();
    }

    public class Timer : ITimer
    {
        private readonly TimeSpan duration;
        private readonly Action callback;
        private readonly CancellationTokenSource cts;
        private readonly Task timerTask;

        private Timer(TimeSpan duration, Action callback, bool runOnce)
        {
            this.duration = duration;
            this.callback = callback;
            this.cts = new CancellationTokenSource();

            // Start the asynchronous timer task immediately
            timerTask = runOnce ? RunOnceAsync() : RunAsync();
        }

        public static Timer TimeFunc(TimeSpan duration, Action callback)
        {
            return new Timer(duration, callback, runOnce: false);
        }

        public static Timer OnceTimeFunc(TimeSpan duration, Action callback)
        {
            return new Timer(duration, callback, runOnce: true);
        }

        public async Task StopAsync()
        {
            cts.Cancel();
            await timerTask;
        }

        private async Task RunAsync()
        {
            try
            {
                while (!cts.Token.IsCancellationRequested)
                {
#if UNITY_WEBGL && !UNITY_EDITOR
                    await UniTask.Delay(duration, cancellationToken: cts.Token);
#else
                    await Task.Delay(duration, cts.Token);
#endif
                    SafeCall(callback);
                }
            }
            catch (OperationCanceledException)
            {
                // Timer was canceled.
            }
        }

        private async Task RunOnceAsync()
        {
            try
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                await UniTask.Delay(duration, cancellationToken: cts.Token);
#else
                await Task.Delay(duration, cts.Token);
#endif
                SafeCall(callback);
            }
            catch (OperationCanceledException)
            {
                // Timer was canceled.
            }
        }

        private void SafeCall(Action callback)
        {
            try
            {
                callback?.Invoke();
            }
            catch (Exception ex)
            {
                Logger.Error($"Recovered from exception: {ex}");
            }
        }

        // A simple check to see if the timer has been stopped.
        public void Check()
        {
            if (cts.IsCancellationRequested)
            {
                // Timer has been stopped.
            }
            else
            {
                // Timer is still running.
            }
        }
    }
}
