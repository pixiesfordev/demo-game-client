using Cysharp.Threading.Tasks;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Shiko.Internal.Socket.Dialer;

#nullable enable
namespace Shiko.Internal.Socket.Udp
{
    internal class Udp : IDialer
    {
        private UdpClient _client;
        private IPEndPoint _remoteEndPoint;
        private TimeSpan _heartbeat = TimeSpan.FromSeconds(60);
        private System.Threading.Timer? _heartbeatTimer;
        private readonly object _writeLock = new object();

        public Udp(string ip, int port)
        {
            try
            {
                _remoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
                _client = new UdpClient();
                _client.Connect(_remoteEndPoint);
            }
            catch (ArgumentNullException ex)
            {
                Logger.Logger.Error($"Argument null exception: {ex.Message}");
                throw new Exception("An error occurred while initializing the UdpClient due to an argument issue.", ex);
            }
            catch (SocketException ex)
            {
                Logger.Logger.Error($"Socket exception: {ex.Message}");
                throw new Exception("An error occurred while initializing the UdpClient due to a socket issue.", ex);
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Logger.Error($"ObjectDisposed exception: {ex.Message}");
                throw new Exception("An object disposed error occurred while initializing the UdpClient.", ex);
            }
        }

        public void SendPacket(Packet.Packet packet)
        {
            lock (_writeLock)
            {
                SafeCall(() =>
                {
                    byte[] data = packet.ToBytes();
                    _client.Send(data, data.Length);
                });
            }
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        public async UniTask<Packet.Packet> ReceivePacketAsync()
#else
        public async Task<Packet.Packet> ReceivePacketAsync()
#endif
        {
            UdpReceiveResult result = await _client.ReceiveAsync();

            // Check valid header size
            if (result.Buffer.Length < Packet.Packet.HeaderSize)
                throw new InvalidOperationException("Received data is smaller than the header size.");

            // Read header
            var packet = new Packet.Packet();
            byte[] headerBytes = new byte[Packet.Packet.HeaderSize];
            Array.Copy(result.Buffer, 0, headerBytes, 0, Packet.Packet.HeaderSize);
            packet.ReadHeader(headerBytes);

            // Calculate data length and ensure data size doesn't go out of bounds
            int dataLength = packet.Length;
            if (result.Buffer.Length < Packet.Packet.HeaderSize + dataLength)
                throw new InvalidOperationException("Received data is smaller than expected.");

            // Read data
            byte[] dataBytes = new byte[dataLength];
            Array.Copy(result.Buffer, Packet.Packet.HeaderSize, dataBytes, 0, dataLength);
            packet.Data = dataBytes;

            return packet;
        }

        public void Close()
        {
            StopHeartBeat();
            _heartbeatTimer?.Dispose();
            _client.Close();
        }

        public EndPoint? LocalAddr()
        {
            return _client.Client.LocalEndPoint;
        }

        public EndPoint? RemoteAddr()
        {
            return _remoteEndPoint;
        }

        public void SetHeartBeat(TimeSpan heartbeat)
        {
            if (heartbeat <= TimeSpan.Zero)
                return;

            _heartbeat = heartbeat;
        }

        public void StartHeartBeat()
        {
            _heartbeatTimer?.Dispose();
            _heartbeatTimer = new System.Threading.Timer(HeartBeat, null, _heartbeat, _heartbeat);
        }

        public void StopHeartBeat()
        {
            if (_heartbeatTimer != null)
            {
                _heartbeatTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _heartbeatTimer.Dispose();
                _heartbeatTimer = null;
            }
        }

        public bool IsConnected()
        {
            try
            {
                return _client.Client != null && _client.Client.Connected;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
        }

        private void HeartBeat(object? state)
        {
            if (!IsConnected())
            {
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

        private void SafeCall(Action callback)
        {
            try
            {
                callback?.Invoke();
            }
            catch (Exception ex)
            {
                Logger.Logger.Error($"Recovered from exception: {ex}");
            }
        }
    }
}