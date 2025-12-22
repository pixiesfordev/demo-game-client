using System;
using System.Collections.Generic;
using Shiko.Serialize;

#nullable enable
namespace Shiko.Config
{
    /// <summary>
    /// Configuration class for socket connections. 
    /// Includes settings for reconnection, serialization, and specific TCP/UDP configurations.
    /// </summary>
    public class SocketConfig
    {
        /// <summary>
        /// Indicates whether the socket should automatically attempt to reconnect after disconnection.
        /// </summary>
        public bool Reconnect { get; set; }

        /// <summary>
        /// The serializer to use for data serialization and deserialization (e.g., JSON, Protobuf, Flatbuffers). 
        /// If not provided, defaults to a JSON serializer.
        /// </summary>
        public ISerializer? Serializer { get; set; }

        /// <summary>
        /// Configuration for TCP connections. If null, TCP connections are disabled.
        /// </summary>
        public TCPConfig? TCP { get; set; }

        /// <summary>
        /// Configuration for UDP connections. If null, UDP connections are disabled.
        /// </summary>
        public UDPConfig? UDP { get; set; }

        /// <summary>
        /// Callback action invoked when a connection is successfully established.
        /// </summary>
        public Action? OnConnect { get; set; }

        /// <summary>
        /// Callback action invoked when the connection is disconnected.
        /// </summary>
        public Action? OnDisconnect { get; set; }

        /// <summary>
        /// Callback action invoked when the socket is closed.
        /// </summary>
        public Action? OnClose { get; set; }
    }

    /// <summary>
    /// Configuration for TCP-based connections, including optional WebSocket and TLS settings.
    /// </summary>
    public class TCPConfig
    {
        /// <summary>
        /// The IP address to connect to. Defaults to "localhost" if not provided.
        /// </summary>
        public string? IP { get; set; }

        /// <summary>
        /// The port number for the TCP connection.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// The interval for sending heartbeat packets to maintain the connection.
        /// </summary>
        public TimeSpan Heartbeat { get; set; }

        /// <summary>
        /// Configuration for WebSocket connections over TCP. If null, WebSocket is disabled.
        /// </summary>
        public WebSocketConfig? WebSocket { get; set; }

        /// <summary>
        /// Configuration for TLS encryption over TCP. If null, TLS is disabled.
        /// </summary>
        public TLSConfig? TLS { get; set; }
    }

    /// <summary>
    /// Configuration for UDP-based connections, including optional DTLS settings.
    /// </summary>
    public class UDPConfig
    {
        /// <summary>
        /// The IP address to connect to.
        /// </summary>
        public string? IP { get; set; }

        /// <summary>
        /// The port number for the UDP connection.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// The interval for sending heartbeat packets to maintain the connection.
        /// </summary>
        public TimeSpan Heartbeat { get; set; }

        /// <summary>
        /// Configuration for DTLS encryption over UDP. If null, DTLS is disabled.
        /// </summary>
        public DTLSConfig? DTLS { get; set; }
    }

    /// <summary>
    /// Configuration for WebSocket connections over TCP.
    /// </summary>
    public class WebSocketConfig
    {
        /// <summary>
        /// The path for the WebSocket connection. Defaults to "/ws".
        /// </summary>
        public string Path { get; set; } = "/ws";

        /// <summary>
        /// Indicates whether the WebSocket connection should use a secure (wss) protocol.
        /// </summary>
        public bool Secure { get; set; }
    }

    /// <summary>
    /// Configuration for TLS (Transport Layer Security) encryption for TCP connections.
    /// </summary>
    public class TLSConfig
    {
        /// <summary>
        /// List of CA (Certificate Authority) file paths used for validating the server's certificate.
        /// </summary>
        public List<string> CaFiles { get; set; } = new List<string>();
    }

    /// <summary>
    /// Configuration for DTLS (Datagram Transport Layer Security) encryption for UDP connections.
    /// </summary>
    public class DTLSConfig
    {
        /// <summary>
        /// List of CA (Certificate Authority) file paths used for validating the server's certificate.
        /// </summary>
        public List<string> CaFiles { get; set; } = new List<string>();
    }
}
