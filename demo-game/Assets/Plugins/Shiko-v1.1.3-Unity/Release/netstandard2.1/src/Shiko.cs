using System;
using Shiko.Config;

namespace Shiko
{
    public interface IClient
    {
        /// <summary>
        /// Opens a route to listen for responses.
        /// </summary>
        /// <param name="route">The route to listen for.</param>
        /// <param name="callback">The callback to handle the response.</param>
        public void On(string route, Action<Context.Context> callback);

        /// <summary>
        /// Requests data via TCP to the server and handles the response data with the callback.
        /// </summary>
        /// <param name="route">The route to send the request to.</param>
        /// <param name="msg">The message to send to the server.</param>
        /// <param name="callback">The callback to handle the response.</param>
        public void Request(string route, object msg, Action<Context.Context> callback);

        /// <summary>
        /// Requests data via UDP to the server and handles the response data with the callback.
        /// </summary>
        /// <param name="route">The route to send the request to.</param>
        /// <param name="msg">The message to send to the server.</param>
        /// <param name="callback">The callback to handle the response.</param>
        public void RequestUDP(string route, object msg, Action<Context.Context> callback);

        /// <summary>
        /// Closes the TCP and UDP connection.
        /// </summary>
        public void Close();
    }

    public class Shiko : IClient
    {
        private Client.Client? _client;

        /// <summary>
        /// Initializes a new instance of the Shiko client with the specified configuration.
        /// </summary>
        /// <param name="config">The socket configuration for the client.</param>
        public Shiko(SocketConfig config)
        {
            _client = new Client.Client(config);
        }

        /// <summary>
        /// Opens a route to listen for responses.
        /// </summary>
        /// <param name="route">The route to listen for.</param>
        /// <param name="callback">The callback to handle the response.</param>
        public void On(string route, Action<Context.Context> callback)
        {
            _client?.On(route, callback);
        }

        /// <summary>
        /// Requests data via TCP to the server and handles the response data with the callback.
        /// </summary>
        /// <param name="route">The route to send the request to.</param>
        /// <param name="msg">The message to send to the server.</param>
        /// <param name="callback">The callback to handle the response.</param>
        public void Request(string route, object msg, Action<Context.Context> callback)
        {
            _client?.Request(route, msg, callback);
        }

        /// <summary>
        /// Requests data via UDP to the server and handles the response data with the callback.
        /// </summary>
        /// <param name="route">The route to send the request to.</param>
        /// <param name="msg">The message to send to the server.</param>
        /// <param name="callback">The callback to handle the response.</param>
        public void RequestUDP(string route, object msg, Action<Context.Context> callback)
        {
            _client?.RequestUDP(route, msg, callback);
        }

        /// <summary>
        /// Closes the TCP and UDP connection.
        /// </summary>
        public void Close()
        {
            _client?.Close();
        }
    }
}