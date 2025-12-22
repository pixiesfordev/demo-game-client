using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Shiko.Config;
using Shiko.Serialize;
using Shiko.Internal.Logger;
using Shiko.Internal.Requester;
using Shiko.Internal.Serializer;
using Shiko.Internal.Socket.Dialer;

#nullable enable
namespace Shiko.Client {
    internal partial class Client {
        private Requester _requester;
        private SocketConfig _config { get; set; }

        private bool _running = false;
        private const int ReconnectInterval = 5000; // 5 seconds

        public Client(SocketConfig config) {
            _config = config;

            // Setup serializer
            if (config.Serializer != null)
                Serializer.SetSerializer(config.Serializer);

            // Setup connections
            IDialer? tcpClient = config.TCP != null ? SetupTCPDialer(config.TCP) : null;
            IDialer? udpClient = config.UDP != null ? SetupUDPDialer(config.UDP) : null;

            // Setup requester
            _requester = new Requester(
                tcpClient, udpClient,
                config.OnConnect, config.OnDisconnect, config.OnClose
            );

            if (_config.Reconnect)
                StartReconnectionTask();
        }

        // Open a route to listen for responses.
        public void On(string route, Action<Context.Context> callback) {
            _requester.On(route, callback);
        }

        // Request data by tcp to the server and handle the response data with the callback.
        public void Request(string route, object msg, Action<Context.Context> callback) {
            _requester.RequestTCP(route, msg, callback);
        }

        // Request data by udp to the server and handle the response data with the callback.
        public void RequestUDP(string route, object msg, Action<Context.Context> callback) {
            _requester.RequestUDP(route, msg, callback);
        }

        // Close tcp and udp connection
        public void Close() {
            _running = false; // Stop reconnection loop
            _requester.Close();
        }

        private IDialer? SetupTCPDialer(TCPConfig config) {
            string ipAddress = string.IsNullOrEmpty(config.IP) ? "localhost" : config.IP;
            int port = config.Port;

            Logger.Debug($"Dialing with TCP to ({ipAddress}:{port})...");

            try {
                IDialer? dialer = null;

                // WebSocket
                if (config.WebSocket != null)
                    dialer = Dialer.DialWebSocket(ipAddress, port, config.WebSocket.Path, config.WebSocket.Secure);
                // TLS
                else if (config.TLS != null)
                    dialer = Dialer.DialTLS(ipAddress, port, LoadCACertificates(config.TLS.CaFiles));
                // TCP
                else dialer = Dialer.DialTCP(ipAddress, port);

                dialer?.SetHeartBeat(config.Heartbeat);
                return dialer;
            } catch (Exception ex) {
                Logger.Error($"An error occurred while setting up the TCP dialer to {ipAddress}:{port}: {ex.Message}");
                return null;
            }
        }

        private IDialer? SetupUDPDialer(UDPConfig config) {
            string ipAddress = string.IsNullOrEmpty(config.IP) ? "localhost" : config.IP;
            int port = config.Port;

            Logger.Debug($"Dialing with UDP to ({ipAddress}:{port})...");

            try {
                IDialer? dialer = null;

                // DTLS
                if (config.DTLS != null)
                    dialer = Dialer.DialDTLS(ipAddress, port, LoadCACertificates(config.DTLS.CaFiles));
                // UDP
                else dialer = Dialer.DialUDP(ipAddress, port);

                dialer?.SetHeartBeat(config.Heartbeat);
                return dialer;
            } catch (Exception ex) {
                Logger.Error($"An error occurred while setting up the UDP dialer to {ipAddress}:{port}: {ex.Message}");
                return null;
            }
        }

        private void StartReconnectionTask() {
            _running = true;
#if UNITY_WEBGL && !UNITY_EDITOR
            UniTask.Void(async () =>
#else
            Task.Run(async () =>
#endif
            {
                while (_running) {
#if UNITY_WEBGL && !UNITY_EDITOR
                    await UniTask.Delay(ReconnectInterval);
#else 
                    await Task.Delay(ReconnectInterval);
#endif

                    if (_config.TCP != null && (_requester.TCPClient() == null || !_requester.TCPClient()!.IsConnected()))
                        TryReconnectTCP();

                    if (_config.UDP != null && (_requester.UDPClient() == null || !_requester.UDPClient()!.IsConnected()))
                        TryReconnectUDP();
                }
            });
        }

        private void TryReconnectTCP() {
            try {
                _requester.TCPClient()?.Close();
                _requester.UpdateTCPClient(SetupTCPDialer(_config.TCP!), true);
            } catch (Exception ex) {
                Logger.Error($"Failed to reconnect TCP: {ex.Message}");
            }
        }

        private void TryReconnectUDP() {
            try {
                _requester.UDPClient()?.Close();
                _requester.UpdateUDPClient(SetupUDPDialer(_config.UDP!), true);
            } catch (Exception ex) {
                Logger.Error($"Failed to reconnect UDP: {ex.Message}");
            }
        }

        private List<X509Certificate2> LoadCACertificates(IEnumerable<string> caFiles) {
            var caCerts = new List<X509Certificate2>();

            foreach (var caFile in caFiles) {
                try {
                    caCerts.Add(new X509Certificate2(caFile));
                } catch (Exception ex) {
                    Logger.Error($"Failed to load CA certificate from file '{caFile}': {ex.Message}");
                }
            }

            return caCerts;
        }
    }
}
