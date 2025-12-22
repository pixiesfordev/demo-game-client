using Cysharp.Threading.Tasks;
using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Shiko.Internal.Socket.Dialer;

#nullable enable
namespace Shiko.Internal.Socket.Tls
{
    internal class Tls : IDialer
    {
        private TcpClient _client;
        private SslStream _stream;
        private TimeSpan _heartbeat;
        private System.Threading.Timer? _heartbeatTimer;
        private readonly object _writeLock = new object();

        public Tls(string ip, int port, List<X509Certificate2>? caCerts = null)
        {
            try
            {
                _client = new TcpClient(ip, port);

                _stream = new SslStream(_client.GetStream(), false,
                    new RemoteCertificateValidationCallback(ValidateServerCertificate),
                    null);

                // Append certificates from the list
                X509CertificateCollection certificateCollection = new X509CertificateCollection();
                if (caCerts != null)
                    certificateCollection.AddRange(caCerts.ToArray());

                // Authenticate as client
                _stream.AuthenticateAsClient(ip, certificateCollection, SslProtocols.Tls12, false);
            }
            catch (AuthenticationException ex)
            {
                Logger.Logger.Error($"Authentication exception: {ex.Message}");
                throw new Exception("An error occurred while authenticating the TLS connection.", ex);
            }
            catch (ArgumentException ex)
            {
                Logger.Logger.Error($"Argument exception: {ex.Message}");
                throw new Exception("An error occurred while initializing the TcpClient due to an argument issue.", ex);
            }
            catch (SocketException ex)
            {
                Logger.Logger.Error($"Socket exception: {ex.Message}");
                throw new Exception("An error occurred while initializing the TcpClient due to a socket issue.", ex);
            }
            catch (Exception ex)
            {
                Logger.Logger.Error($"Unexpected exception: {ex.Message}");
                throw new Exception("An unexpected error occurred while initializing the TcpClient.", ex);
            }
        }

        public void SendPacket(Packet.Packet packet)
        {
            lock (_writeLock)
            {
                SafeCall(() =>
                {
                    byte[] data = packet.ToBytes();
                    _stream.Write(data, 0, data.Length);
                });
            }
        }

        // Handle sticky packets
#if UNITY_WEBGL && !UNITY_EDITOR
        public async UniTask<Packet.Packet> ReceivePacketAsync()
#else
        public async Task<Packet.Packet> ReceivePacketAsync()
#endif
        {
            // Read header
            byte[] headerBuffer = new byte[Packet.Packet.HeaderSize];
            int bytesRead = await _stream.ReadAsync(headerBuffer, 0, headerBuffer.Length);
            if (bytesRead < Packet.Packet.HeaderSize)
                throw new InvalidOperationException("Failed to read the full header.");

            // Extract header information
            var packet = new Packet.Packet();
            packet.ReadHeader(headerBuffer);

            // Read data (continue reading until all data is received)
            packet.Data = new byte[packet.Length];

            int totalBytesRead = 0;
            while (totalBytesRead < packet.Length)
            {
                int remainingBytes = packet.Length - totalBytesRead;
                int bytesToRead = Math.Min(remainingBytes, 8192);

                // Read the next chunk of data
                int currentBytesRead = await _stream.ReadAsync(packet.Data, totalBytesRead, bytesToRead);
                if (currentBytesRead == 0)
                    throw new InvalidOperationException("Unexpected end of stream while reading packet data.");

                totalBytesRead += currentBytesRead;
            }

            return packet;
        }

        public void Close()
        {
            StopHeartBeat();
            _stream.Close();
            _client.Close();
        }

        public EndPoint? LocalAddr()
        {
            return _client.Client.LocalEndPoint;
        }

        public EndPoint? RemoteAddr()
        {
            return _client.Client.RemoteEndPoint;
        }

        public void SetHeartBeat(TimeSpan heartbeat)
        {
            if (heartbeat <= TimeSpan.Zero)
                return;

            _heartbeat = heartbeat;
        }

        public TimeSpan GetHeartBeat()
        {
            return _heartbeat;
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

            var heartbeat = Array.Empty<byte>();

            SendPacket(
                new Packet.Packet(
                    Packet.Type.HEARTBEAT,
                    heartbeat.Length,
                    heartbeat
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

        // Validate the server certificate
        private static bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            /*
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Logger.Logger.Error($"Server certificate validation failed: {sslPolicyErrors}");
            */

            return true; // Return true to accept any server certificate
        }
    }
}
