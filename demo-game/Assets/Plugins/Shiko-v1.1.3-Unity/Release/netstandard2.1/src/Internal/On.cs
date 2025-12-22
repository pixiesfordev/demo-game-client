using System;
using System.Collections.Concurrent;

namespace Shiko.Internal.On
{
    internal class On
    {
        private readonly ConcurrentDictionary<string, Action<Context.Context>> onListenRoutes = new ConcurrentDictionary<string, Action<Context.Context>>();
        private readonly ConcurrentDictionary<string, Action<Context.Context>> onListenEvents = new ConcurrentDictionary<string, Action<Context.Context>>();

        public On() { }

        public void OnListenRoute(string route, Action<Context.Context> callback)
        {
            onListenRoutes.TryAdd(route, callback);
        }

        public void OnListenEvent(string eventName, Action<Context.Context> callback)
        {
            onListenEvents.TryAdd(eventName, callback);
        }

        public void OnRoute(string route, byte[] data)
        {
            if (onListenRoutes.TryGetValue(route, out var callback))
                SafeCall(callback, new Context.Context(data));
        }

        public void OnEvent(string eventName, byte[] data)
        {
            if (onListenEvents.TryGetValue(eventName, out var callback))
                SafeCall(callback, new Context.Context(data));
        }

        private void SafeCall(Action<Context.Context> callback, Context.Context context)
        {
            try
            {
                callback?.Invoke(context);
            }
            catch (Exception ex)
            {
                Logger.Logger.Error($"{nameof(On)}: Exception in callback: {ex.Message}");
                Logger.Logger.Error($"{nameof(On)}: Stack trace: {ex.StackTrace}");
            }
        }
    }
}
