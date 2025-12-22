using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Shiko.Internal.Socket.Dialer;
using Shiko.Internal.Once;

#nullable enable
namespace Shiko.Internal.Requester {
    internal class Requester {
        private string _uid;
        private On.On _on;
        private IDialer? _tcpClient;
        private IDialer? _udpClient;
        private static long _requestCounter = 0;

        // Once callbacks
        private Once.Once _onConnect { get; set; }
        private Once.Once _onDisconnect { get; set; }
        private Once.Once _onClose { get; set; }

        public Requester(IDialer? tcpClient, IDialer? udpClient, Action? onConnect, Action? onDisconnect, Action? onClose) {
            _uid = Unique.Unique.Generate(Unique.Unique.Generate("uid"));
            _on = new On.On();
            _onConnect = new Once.Once(onConnect);
            _onDisconnect = new Once.Once(onDisconnect);
            _onClose = new Once.Once(onClose);

            UpdateTCPClient(tcpClient, false);
            UpdateUDPClient(udpClient, false);
        }

        public IDialer? TCPClient() {
            return _tcpClient;
        }

        public IDialer? UDPClient() {
            return _udpClient;
        }

        public void UpdateTCPClient(IDialer? tcpClient, bool disconnected) {
            UpdateClient(ref _tcpClient, tcpClient);
        }

        public void UpdateUDPClient(IDialer? udpClient, bool disconnected) {
            UpdateClient(ref _udpClient, udpClient);
        }

        private void UpdateClient(ref IDialer? currentClient, IDialer? newClient) {
            currentClient?.Close();
            currentClient = newClient;

            // Reset the on disconnect callback
            _onDisconnect.Reset();

            if (currentClient != null)
                InitializeClient(currentClient);
        }

        private void InitializeClient(IDialer client) {
            StartReceiving(client);
#if UNITY_WEBGL && !UNITY_EDITOR
            // Use UniTask (single-threaded) to wait for the client to connect, 
            // with a timeout, before performing the handshake.
            UniTask.Void(async() =>
            {
                try
                {
                    await UniTask.WaitUntil(() => client.IsConnected())
                        .Timeout(TimeSpan.FromSeconds(10));

                    Handshake(client);
                    client.StartHeartBeat();
                }
                catch (TimeoutException) 
                {
                    Logger.Logger.Error("Waiting timeout for establishing connection before handshake");
                    return;
                }
            });
#else
            Handshake(client);
            client.StartHeartBeat();
#endif
        }

        public void On(string route, Action<Context.Context> callback) {
            _on.OnListenRoute(route, callback);
        }

        public void RequestTCP(string route, object msg, Action<Context.Context> callback) {
            if (_tcpClient == null) {
                Logger.Logger.Error("TCP dialer not initialized or disconnected.");
                return;
            }

            Request(_tcpClient, route, msg, callback);
        }

        public void RequestUDP(string route, object msg, Action<Context.Context> callback) {
            if (_udpClient == null) {
                Logger.Logger.Error("UDP dialer not initialized or disconnected.");
                return;
            }

            Request(_udpClient, route, msg, callback);
        }

        private void Request(IDialer dialer, string route, object msg, Action<Context.Context> callback) {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(ToMessage(msg)));

            var request = new Packet.Request(
                $"{_uid}-{Interlocked.Increment(ref _requestCounter)}",
                route,
                bytes
            );

            _on.OnListenEvent(request.MID, callback);

            byte[] data = request.Marshal(Serializer.Serializer.GetSerializer());

            dialer.SendPacket(
                new Packet.Packet(
                    Packet.Type.DATA,
                    data.Length,
                    data
                )
            );
        }

        private static object ToMessage(object msg) {
            if (msg == null)
                throw new ArgumentNullException(nameof(msg), "Message cannot be null");

            Type type = msg.GetType();
            TypeCode typeCode = Type.GetTypeCode(type);

            switch (typeCode) {
                case TypeCode.Object:
                    // byte[]
                    if (type == typeof(byte[]))
                        return new Packet.Message { Raw = (byte[])msg };

                    // class
                    else if (type.IsClass || (type.IsValueType && !type.IsPrimitive))
                        return msg;

                    break;

                // string
                case TypeCode.String:
                    return new Packet.Message { Raw = System.Text.Encoding.UTF8.GetBytes((string)msg) };
            }

            throw new ArgumentException("Message must be of type byte[], string, or a class", nameof(msg));
        }

        private void Handshake(IDialer dialer) {
            var handshake = new Packet.Handshake(_uid);
            var data = handshake.Marshal(Serializer.Serializer.GetSerializer());

            dialer.SendPacket(
                new Packet.Packet(
                    Packet.Type.HANDSHAKE,
                    data.Length,
                    data
                )
            );
        }

        private void StartReceiving(IDialer dialer) {
            Logger.Logger.Debug($"Start receiving message from ({dialer.RemoteAddr()})");

#if UNITY_WEBGL && !UNITY_EDITOR
            UniTask.Void(async () =>
#else        
            Task.Run(async () =>
#endif
            {
                while (true) {
                    try {
                        Packet.Packet packet = await dialer.ReceivePacketAsync();
                        ProcessPacket(dialer, packet);
                    } catch (OperationCanceledException) {
                        // 主動取消不報錯誤
                        break;
                    } catch (Exception ex) {
                        Logger.Logger.Error($"Error receiving packet: {ex}");
                        _onDisconnect.Call();
                        dialer.Close();
                        break;
                    }
                }
            });
        }

        private void ProcessPacket(IDialer dialer, Packet.Packet packet) {
            Logger.Logger.Trace($"Received packet: ({packet.Info()})");

            switch (packet.Type) {
                case Packet.Type.ACK:
                    // Handshake again if failed to handle ack
                    if (!HandleAck(dialer, packet))
                        Handshake(dialer);
                    break;

                case Packet.Type.DATA:
                    HandleData(packet);
                    break;

                case Packet.Type.KICK:
                    Logger.Logger.Info("Received Kick packet. Closing connections.");
                    _onClose.Call();
                    dialer.Close();
                    break;

                case Packet.Type.HANDSHAKE:
                case Packet.Type.HEARTBEAT:
                    // No need to handle
                    break;

                default:
                    Logger.Logger.Warn($"Received unkown packet type. ({packet.Info()})");
                    break;
            }
        }

        private bool HandleAck(IDialer dialer, Packet.Packet packet) {
            var ack = new Packet.Ack();
            ack.Unmarshal(Serializer.Serializer.GetSerializer(), packet.Data);

            if (_uid != ack.UID) {
                Logger.Logger.Error($"Unmatched uid: ({ack.UID})");
                return false;
            }

            Logger.Logger.Info($"Ack with UID: ({ack.UID}), Remote address: {dialer.RemoteAddr()}, Session established.");

            _onConnect.Call();

            return true;
        }

        private void HandleData(Packet.Packet packet) {
            var response = new Packet.Response();
            response.Unmarshal(Serializer.Serializer.GetSerializer(), packet.Data);

            Logger.Logger.Trace($"Received response: ({response.Info()})");

            _on.OnRoute(response.Route, response.Data);
            _on.OnEvent(response.Route, response.Data);
        }

        // Close both tcp and udp connection
        public void Close() {
            _onClose.Call();

            _tcpClient?.Close();
            _udpClient?.Close();
        }
    }
}