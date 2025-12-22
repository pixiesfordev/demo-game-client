using Cysharp.Threading.Tasks;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.Security;
using Shiko.Internal.Socket.Dialer;

namespace Shiko.Internal.Socket.Dtls
{
    internal class Dtls : IDialer
    {
        private readonly UdpClient _udpClient;
        private readonly DtlsTransport _dtlsTransport;
        private readonly IPEndPoint _remoteEndPoint;
        private readonly IPEndPoint _localEndPoint;
        private TimeSpan _heartbeat;
        private System.Threading.Timer? _heartbeatTimer;
        private readonly object _writeLock = new object();

        public Dtls(string ip, int port, List<X509Certificate2>? caCerts = null)
        {
            try
            {
                _remoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
                _udpClient = new UdpClient(ip, port);

                var dtlsClientProtocol = new DtlsClientProtocol(new SecureRandom());
                var dtlsClient = new DtlsClient(caCerts);
                var transport = new UdpTransport(_udpClient);

                _dtlsTransport = dtlsClientProtocol.Connect(dtlsClient, transport);
                _localEndPoint = (IPEndPoint)_udpClient.Client.LocalEndPoint!;
            }
            catch (AggregateException ex)
            {
                Logger.Logger.Error($"AggregateException: {ex.Message}");
                throw new Exception("An error occurred while setting up the DTLS connection.", ex);
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
            catch (Exception ex)
            {
                Logger.Logger.Error($"Exception: {ex.Message}");
                throw new Exception("An error occurred while setting up the DTLS connection.", ex);
            }
        }

        public void SendPacket(Packet.Packet packet)
        {
            lock (_writeLock)
            {
                SafeCall(() =>
                {
                    byte[] data = packet.ToBytes();
                    _dtlsTransport.Send(data, 0, data.Length);
                });
            }
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        public async UniTask<Packet.Packet> ReceivePacketAsync()
#else
        public async Task<Packet.Packet> ReceivePacketAsync()
#endif
        {
            // Receive data asynchronously
            // Maximum UDP packet size (65535 - IP header - UDP header)
            var buffer = new byte[65507];

            // Block receiving by waiting indefinitely
#if UNITY_WEBGL && !UNITY_EDITOR
            int length = await UniTask.RunOnThreadPool(() => _dtlsTransport.Receive(buffer, 0, buffer.Length, -1));       
#else
            int length = await Task.Run(() => _dtlsTransport.Receive(buffer, 0, buffer.Length, -1));
#endif

            // Check valid header size
            if (length < Packet.Packet.HeaderSize)
                throw new InvalidOperationException("Received data is smaller than the header size.");

            // Read header
            var packet = new Packet.Packet();
            byte[] headerBytes = new byte[Packet.Packet.HeaderSize];
            Array.Copy(buffer, 0, headerBytes, 0, Packet.Packet.HeaderSize);
            packet.ReadHeader(headerBytes);

            // Calculate data length and ensure data size doesn't go out of bounds
            int dataLength = packet.Length;
            if (length < Packet.Packet.HeaderSize + dataLength)
                throw new InvalidOperationException("Received data is smaller than expected.");

            // Read data
            byte[] dataBytes = new byte[dataLength];
            Array.Copy(buffer, Packet.Packet.HeaderSize, dataBytes, 0, dataLength);
            packet.Data = dataBytes;

            return packet;
        }

        public void Close()
        {
            _dtlsTransport.Close();
            _udpClient.Close();
        }

        public EndPoint? LocalAddr() => _localEndPoint;

        public EndPoint? RemoteAddr() => _remoteEndPoint;

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
                return _udpClient.Client != null && _udpClient.Client.Connected;
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

    }

    internal class DtlsClient : DefaultTlsClient
    {
        public override ProtocolVersion MinimumVersion => ProtocolVersion.DTLSv12;
        public override ProtocolVersion ClientVersion => ProtocolVersion.DTLSv12;
        private readonly List<X509Certificate2>? _caCerts;

        public DtlsClient(List<X509Certificate2>? caCerts)
        {
            _caCerts = caCerts;
        }

        public override int[] GetCipherSuites()
        {
            // Use the same cipher suites as the server side
            return new int[]
            {
            CipherSuite.TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256,
            CipherSuite.TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256,
            };
        }

        public override TlsAuthentication GetAuthentication()
        {
            return new DtlsAuthentication(_caCerts, true);
        }
    }

    // If the handshake is successful and the server certificate is correct, you will receive a NotifyServerCertificate. 
    // If mutual encryption verification is required, you need to implement the GetClientCredentials method. 
    // Otherwise, you can simply return null.
    internal class DtlsAuthentication : TlsAuthentication
    {
        private readonly List<X509Certificate2>? _caCerts;
        private readonly bool _validateServerCertificate;

        public DtlsAuthentication(List<X509Certificate2>? caCerts, bool validateServerCertificate)
        {
            _caCerts = caCerts;
            _validateServerCertificate = validateServerCertificate;
        }

        public void NotifyServerCertificate(Certificate serverCertificate)
        {
            if (_validateServerCertificate == true && _caCerts != null)
            {
                var parser = new X509CertificateParser();
                foreach (var cert in serverCertificate.GetCertificateList())
                {
                    var x509Cert = parser.ReadCertificate(cert.GetEncoded());
                    if (!x509Cert.IsValidNow)
                        throw new TlsFatalAlert(AlertDescription.bad_certificate);
                }
            }
        }

        public TlsCredentials? GetClientCredentials(Org.BouncyCastle.Crypto.Tls.CertificateRequest certificateRequest)
        {
            if (_caCerts == null || _caCerts.Count == 0)
                return null; // No client certificates available

            try
            {
                // List to hold X509CertificateStructure instances
                var certStructures = new List<X509CertificateStructure>();

                // Parse each certificate and add its X509CertificateStructure
                foreach (var clientCert in _caCerts)
                {
                    var parser = new X509CertificateParser();
                    var bcCert = parser.ReadCertificate(clientCert.RawData);
                    certStructures.Add(bcCert.CertificateStructure);
                }

                // Create a certificate chain from all the certificate structures
                var certChain = new Certificate(certStructures.ToArray());

                // Get the private key from the X509Certificate2
                AsymmetricKeyParameter privateKey = DotNetUtilities.GetKeyPair(_caCerts[0].GetRSAPrivateKey()).Private;

                // Create and return TlsCredentials
                return new DefaultTlsSignerCredentials(null, certChain, privateKey,
                    new SignatureAndHashAlgorithm(HashAlgorithm.sha256, SignatureAlgorithm.rsa));
            }
            catch (Exception ex)
            {
                Logger.Logger.Error($"Error creating client credentials: {ex.Message}");
                return null;
            }
        }
    }

    internal class UdpTransport : DatagramTransport
    {
        private UdpClient _udpClient;
        int _maxBufferSize = 65507;

        public UdpTransport(UdpClient udpClient)
        {
            _udpClient = udpClient;
        }

        public int Receive(byte[] buf, int off, int len, int waitMillis)
        {
            try
            {
                var resultTask = _udpClient.ReceiveAsync();
                if (resultTask.Wait(waitMillis))
                {
                    var result = resultTask.Result;
                    int resultLength = result.Buffer.Length;
                    if (resultLength > len)
                        throw new Org.BouncyCastle.Crypto.DataLengthException($"result length ({resultLength}) > buffer size ({len})");

                    Buffer.BlockCopy(result.Buffer, 0, buf, off, resultLength);
                    return resultLength;
                }

                return 0;
            }
            catch (Exception ex)
            {
                Logger.Logger.Error($"Receive error: {ex.Message}");
                throw;
            }
        }

        public void Send(byte[] buf, int off, int len)
        {
            _udpClient.Send(buf, len);
        }

        public int GetReceiveLimit() => _maxBufferSize;

        public int GetSendLimit() => _maxBufferSize;

        public void Close() => _udpClient.Close();
    }
}
