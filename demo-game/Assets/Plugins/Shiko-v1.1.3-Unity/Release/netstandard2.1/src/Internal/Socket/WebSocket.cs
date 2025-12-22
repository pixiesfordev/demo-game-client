using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Shiko.Internal.Socket.Dialer;
using Shiko.Internal.Socket.WebSocketLib;
using Cysharp.Threading.Tasks;

#nullable enable
namespace Shiko.Internal.Socket.WebSocket {
    internal class WebSocket : IDialer {
        private WebSocketLib.WebSocket _client;
        private Uri _remoteUri;
        private TimeSpan _heartbeat;
        private TimeSpan _connectionTimeout = TimeSpan.FromSeconds(3);
        private readonly object _writeLock = new object();

        // Message queue to handle received packets
        private Queue<Packet.Packet> _receiveQueue = new Queue<Packet.Packet>();
        private readonly SemaphoreSlim _queueSemaphore = new SemaphoreSlim(0, int.MaxValue);
        private readonly object _queueLock = new object();

        private Timer.Timer? _heartbeatTimer;
#if !UNITY_WEBGL || UNITY_EDITOR
        private Timer.Timer? _dispatchTimer;
#endif
        private volatile bool disposed = false;

        public WebSocket(string ip, int port, string path, bool secure) {
            var scheme = secure ? "wss" : "ws";
            _remoteUri = new Uri($"{scheme}://{ip}:{port}{path}");

            _client = new WebSocketLib.WebSocket(_remoteUri.ToString());

            // Set up WebSocket events
            _client.OnOpen += OnOpen;
            _client.OnMessage += OnMessage;
            _client.OnClose += OnClose;
            _client.OnError += OnError;

#if UNITY_WEBGL && !UNITY_EDITOR
            _client.Connect();
#else
            _client.Connect().Wait(_connectionTimeout);
            _dispatchTimer = Timer.Timer.TimeFunc(
                TimeSpan.FromMilliseconds(16),
                _client.DispatchMessageQueue
            );
#endif
        }

        private void OnOpen() {
#if UNITY_WEBGL && !UNITY_EDITOR
            Logger.Logger.Info("(WebGL) WebSocket connected.");
#else
            Logger.Logger.Info("(Non-WebGL) WebSocket connected.");
#endif
        }

        private void OnMessage(byte[] data) {
            var packet = new Packet.Packet();
            packet.ReadHeader(data.AsSpan(0, Packet.Packet.HeaderSize).ToArray());
            packet.Data = data.AsSpan(Packet.Packet.HeaderSize).ToArray();

            // Add the packet to the queue in a thread-safe manner
            lock (_queueLock) {
                _receiveQueue.Enqueue(packet);
            }

            // Signal that a new packet is available
            _queueSemaphore.Release();
        }

        private void OnClose(WebSocketLib.WebSocketCloseCode closeCode) {
#if UNITY_WEBGL && !UNITY_EDITOR
            Logger.Logger.Info($"(WebGL) Closed: StatusCode: {closeCode}");
#else
            Logger.Logger.Info($"(Non-WebGL) Closed: StatusCode: {closeCode}");
#endif
        }

        private void OnError(string errorMsg) {
#if UNITY_WEBGL && !UNITY_EDITOR
            Logger.Logger.Error($"(WebGL) WebSocket error: {errorMsg}");
#else
            Logger.Logger.Error($"(Non-WebGL) WebSocket error: {errorMsg}");
#endif
        }

        public void SendPacket(Packet.Packet packet) {
            lock (_writeLock) {
                SafeCall(() => {
                    var data = packet.ToBytes();
                    _client.Send(data);
                });
            }
        }



#if UNITY_WEBGL && !UNITY_EDITOR
        public async UniTask<Packet.Packet> ReceivePacketAsync() {
            if (disposed) throw new OperationCanceledException();
            await UniTask.WaitUntil(() => _receiveQueue.Count > 0 || disposed);
            if (disposed) throw new OperationCanceledException();
            Packet.Packet packet;
            lock (_queueLock) {
                packet = _receiveQueue.Dequeue();
            }
            return packet;
        }
#else
        public async Task<Packet.Packet> ReceivePacketAsync() {
            if (disposed) throw new OperationCanceledException();
            await _queueSemaphore.WaitAsync();
            if (disposed) throw new OperationCanceledException();
            Packet.Packet packet;
            lock (_queueLock) {
                if (_receiveQueue.Count == 0) throw new OperationCanceledException();
                packet = _receiveQueue.Dequeue();
            }
            return packet;
        }
#endif

        public void Close() {
            if (disposed) return;
            disposed = true;


#if UNITY_WEBGL && !UNITY_EDITOR 
            _client.Close(WebSocketCloseCode.Normal, "");
#else
            _client.CancelConnection();
            _dispatchTimer?.StopAsync();
            _dispatchTimer = null;
#endif

            StopHeartBeat();

            lock (_queueLock) { _receiveQueue.Clear(); }
            if (_queueSemaphore.CurrentCount == 0) _queueSemaphore.Release();
        }

        public EndPoint? LocalAddr() {
            // Return local address if needed
            return null;
        }

        public EndPoint? RemoteAddr() {
            // Return remote address if needed
            return null;
        }

        public void SetHeartBeat(TimeSpan heartbeat) {
            if (heartbeat <= TimeSpan.Zero)
                return;

            _heartbeat = heartbeat;
        }

        public TimeSpan GetHeartBeat() => _heartbeat;

        public void StartHeartBeat() {
            _heartbeatTimer = Timer.Timer.TimeFunc(_heartbeat, HeartBeat);
        }

        public void StopHeartBeat() {
            _heartbeatTimer?.StopAsync();
            _heartbeatTimer = null;
        }

        public bool IsConnected() => _client != null && _client.State == WebSocketState.Open;

        private void HeartBeat() {
            if (!IsConnected()) {
                StopHeartBeat();
                return;
            }

            var hearbeat = Array.Empty<byte>();

            SendPacket(
                new Packet.Packet(
                    Packet.Type.HEARTBEAT,
                    hearbeat.Length,
                    hearbeat
                )
            );
        }

        private void SafeCall(Action callback) {
            try {
                callback?.Invoke();
            } catch (Exception ex) {
                Logger.Logger.Error($"Recovered from exception: {ex}");
            }
        }
    }
}
