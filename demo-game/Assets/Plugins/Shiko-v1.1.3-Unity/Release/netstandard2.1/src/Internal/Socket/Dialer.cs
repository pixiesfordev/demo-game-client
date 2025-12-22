using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

#nullable enable
namespace Shiko.Internal.Socket.Dialer
{
    internal interface IDialer
    {
        public void SendPacket(Packet.Packet packet);
#if UNITY_WEBGL && !UNITY_EDITOR
        public UniTask<Packet.Packet> ReceivePacketAsync();
#else
        public Task<Packet.Packet> ReceivePacketAsync();
#endif
        public void Close();
        public System.Net.EndPoint? LocalAddr();
        public System.Net.EndPoint? RemoteAddr();
        public void SetHeartBeat(TimeSpan heartbeat);
        void StartHeartBeat();
        bool IsConnected();
    }

    internal static class Dialer
    {
        public static IDialer? DialTCP(string ip, int port)
        {
            try
            {
                return new Tcp.Tcp(ip, port);
            }
            catch (Exception ex)
            {
                Logger.Logger.Error($"Failed to establish TCP connection: {ex}");
                return null;
            }
        }

        public static IDialer? DialUDP(string ip, int port)
        {
            try
            {
                return new Udp.Udp(ip, port);
            }
            catch (Exception ex)
            {
                Logger.Logger.Error($"Failed to establish UDP connection: {ex}");
                return null;
            }
        }

        public static IDialer? DialTLS(string ip, int port, List<X509Certificate2>? cacerts = null)
        {
            try
            {
                return new Tls.Tls(ip, port, cacerts);
            }
            catch (Exception ex)
            {
                Logger.Logger.Error($"Failed to establish TLS connection: {ex}");
                return null;
            }
        }

        public static IDialer? DialDTLS(string ip, int port, List<X509Certificate2>? cacerts = null)
        {
            try
            {
                return new Dtls.Dtls(ip, port, cacerts);
            }
            catch (Exception ex)
            {
                Logger.Logger.Error($"Failed to establish DTLS connection: {ex}");
                return null;
            }
        }

        public static IDialer? DialWebSocket(string ip, int port, string path, bool secure)
        {
            try
            {
                return new WebSocket.WebSocket(ip, port, path, secure);
            }
            catch (Exception ex)
            {
                Logger.Logger.Error($"Failed to establish WebSocket connection: {ex}");
                return null;
            }
        }
    }
}