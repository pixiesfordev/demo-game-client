using System;
using System.Threading;
using System.Collections.Generic;
using Shiko;
using Shiko.Config;
using Shiko.Context;
using Shiko.Serialize.Protobuf;
using UnityEngine;
using System.IO;

public class ShikoClient : MonoBehaviour {
    private Shiko.Shiko _shiko;
    private Action? _onConnect, _onDisconnect, _onClose;

    // Server Certificate
    private static readonly List<string> _caFiles = new List<string>(); //new List<string> { "Assets/AddressableAssets/server.crt" };

    public void ConnectToServer(bool formalServer) {
        // Setup log mode for internal informations
        Shiko.ClientMode.SetMode(Shiko.ClientMode.TestMode);
        // Print console log in unity console
        UnityConsole.Redirect();
        string certPath = Path.Combine(Application.streamingAssetsPath, "server.crt");
        _caFiles.Add(certPath);

        var ip = "";
#if Dev
        ip = (formalServer) ? "waifu-tower.minigames-dev.gamer-dev.com" : "waifu-tower-test.minigames-dev.gamer-dev.com";
#elif Test
        ip = (formalServer) ? "ws-test-waifu-tower.88play.online" : "waifu-tower-test.minigames-dev.gamer-dev.com";
#elif Release
        ip = (formalServer) ? "ws-waifu-tower.epicminigame.com" : "ws-waifu-tower.epicminigame.net";
#endif


        if (string.IsNullOrEmpty(ip)) {
            Debug.LogError("ip為空");
            return;
        }

        var socketConfig = new SocketConfig {
            // Handle reconnection
            //Reconnect = true,
            // Use protobuf serializer
            Serializer = new Protobuf(),
            // TCP config
            TCP = new TCPConfig {
                IP = ip,
                Port = 443,
                Heartbeat = TimeSpan.FromSeconds(10), // Set heartbeat interval
                WebSocket = new WebSocketConfig {
                    Secure = true,
                },
            },
            // Callbacks
            OnConnect = _onConnect,
            OnDisconnect = _onDisconnect,
            OnClose = _onClose,
        };

        _shiko = new Shiko.Shiko(socketConfig);
    }

    // Update is called once per frame
    private void Update() {
        // Implement updates
    }

    // OnDestroy is called when the object is destroyed.
    private void OnDestroy() {
        // Close should be called to stop the reconnection loop.
        if (_shiko != null) _shiko.Close();
    }

    /// <summary>
    //      OnListen registers a callback for the specified route, 
    //      which is triggered when a request is received on that route.
    /// </summary>
    public void OnListen(string route, Action<Context> callback) {
        _shiko.On(route, callback);
    }

    /// <summary>
    ///     OnRequest sends a request to the specified route with the given message,
    ///     and registers a callback to handle the response.
    /// </summary>
    public void OnRequest(string route, object msg, Action<Context> callback) {
        _shiko.Request(route, msg, callback);
    }

    /// <summary>
    ///     OnRequestUDP sends a UDP request to the specified route with the given message,
    ///     and registers a callback to handle the response.
    /// </summary>
    public void OnRequestUDP(string route, object msg, Action<Context> callback) {
        _shiko.RequestUDP(route, msg, callback);
    }

    /// <summary>
    ///     Assigns a callback for when the client successfully connects to the server.
    /// </summary>
    public void OnConnect(Action action) {
        _onConnect = action;
    }

    /// <summary>
    ///     Assigns a callback for when the client disconnects from the server.
    /// </summary>
    public void OnDisconnect(Action action) {
        _onDisconnect = action;
    }

    /// <summary>
    ///     Assigns a callback for when the client connection closes.
    /// </summary>
    public void OnClose(Action action) {
        _onClose = action;
    }

    /// <summary>
    ///     Close closes the connection and session.
    /// </summary>
    public void Close() {
        _shiko.Close();
    }
}
