using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PureSignalRClient.Types;
using PureWebSockets;

namespace PureSignalRClient
{
    public class Client : IDisposable
    {
        private readonly string _rootHost;
        private readonly string _httpHost;
        private readonly string _wsHost;
        private readonly bool _isSecure;
        private PureWebSocket _webSocket;
        private Timer _connectionMonitor;
        private DateTime _lastMessageTime;
        private bool _disconnectCalled;
        private string _connectionId;
        private string _connectionToken;
        private string _lastMessageId;
        private string _groupsToken;
        private readonly string[] _hubs;

        /// <summary>
        ///     Raised when a new message is received
        /// </summary>
        public event MessageReceived OnNewMessage;

        /// <summary>
        ///     Raised when a message has failed to send
        /// </summary>
        public event SendFailed OnSendFailed;

        /// <summary>
        ///     Raised when an error occurs
        /// </summary>
        public event Error OnError;

        /// <summary>
        ///     Raised when binary data is received (should not happen)
        /// </summary>
        public event Data OnData;

        /// <summary>
        ///     Raised when an unrecoverable error has occured. The instance should be declared dead and unusable here.
        /// </summary>
        public event Fatality OnFatality;

        /// <summary>
        ///     Output debug messages
        /// </summary>
        public bool Debug { get; set; }

        /// <summary>
        ///     Creates a new Client
        /// </summary>
        /// <param name="url">the http(s) url of the signalr server to connect to (usually ends in /signalr)</param>
        /// <param name="hubs">the server hubs you wish to subscibe to</param>
        public Client(string url, params string[] hubs)
        {
            _hubs = hubs;

            if (!url.ToLower().StartsWith("http"))
                throw new ArgumentException("invalid url", url);

            if (url.ToLower().StartsWith("https"))
                _isSecure = true;
            var rootHost = url.ToLower().Replace(_isSecure ? "https://" : "http://", "");
            if (rootHost.EndsWith("/"))
                rootHost = rootHost.Substring(0, rootHost.Length - 1);
            _rootHost = rootHost;
            _httpHost = _isSecure ? $"https://{rootHost}" : $"http://{rootHost}";
            _wsHost = _isSecure ? $"wss://{rootHost}" : $"ws://{rootHost}";

            if (Debug)
                WriteLine("New SignalRWsConnection Instance Created");
        }

        /// <summary>
        ///     Connect to the server and start processing messages
        /// </summary>
        public void Connect()
        {
            _webSocket?.Dispose();

            _disconnectCalled = false;

            // get the connection info from the signalr host
            var connInf = SignalR.Negotiate(_httpHost, _hubs);

            // we only work with websockets
            if (!connInf.TryWebSockets)
                throw new WebSocketException(WebSocketError.UnsupportedProtocol,
                    "WebSocket Connections Not Supported By This Host");

            _connectionToken = connInf.ConnectionToken;
            _connectionId = connInf.ConnectionId;

            _webSocket = SignalR.Connect(_wsHost, _connectionToken, _hubs);

            HookupEvents();

            _webSocket.Connect();

            if (Debug)
                WriteLine($"Connect Called, URL: {_rootHost}");
        }

        private void HookupEvents()
        {
            _webSocket.OnClosed += _webSocket_OnClosed;
            _webSocket.OnData += _webSocket_OnData;
            _webSocket.OnError += _webSocket_OnError;
            _webSocket.OnFatality += _webSocket_OnFatality;
            _webSocket.OnMessage += _webSocket_OnMessage;
            _webSocket.OnOpened += _webSocket_OnOpened;
            _webSocket.OnSendFailed += _webSocket_OnSendFailed;
        }

        /// <summary>
        ///     Shutdown / Close the socket connection
        /// </summary>
        public void Disconnect()
        {
            _disconnectCalled = true;
            SignalR.Abort(_httpHost, _connectionToken, _hubs);
            _webSocket?.Disconnect();

            if (Debug)
                WriteLine("Disconnect Called");
        }

        private void _webSocket_OnSendFailed(string data, Exception ex)
        {
            Task.Run(() => { OnSendFailed?.Invoke(data, ex); });

            if (Debug)
                WriteLine($"Send Failed, Data: {data}, Exception: {ex.Message}");
        }

        private void _webSocket_OnOpened()
        {
            _connectionMonitor?.Dispose();

            _connectionMonitor = new Timer(ConnectionMonitorCheck, null, TimeSpan.FromSeconds(30),
                TimeSpan.FromSeconds(30));

            if (Debug)
                WriteLine("Connection Opened");
        }

        private void ConnectionMonitorCheck(object state)
        {
            if (_disposedValue || _disconnectCalled) return;
            if (_lastMessageTime.AddSeconds(30) > DateTime.Now) return;

            if (Debug)
                WriteLine("Connection Timeout, Atempting Reconnect");

            _webSocket.Dispose(false);
            _webSocket = SignalR.Reconnect(_wsHost, _connectionToken, _hubs, _lastMessageId, _groupsToken);

            HookupEvents();

            _webSocket.Connect();
        }

        private void _webSocket_OnMessage(string message)
        {
            try
            {
                if (Debug)
                    WriteLine($"New Message, Message: {message}");

                _lastMessageTime = DateTime.Now;
                // check if this is a keep alive
                if (message == "{{}}") return;
                if (message.Trim() == "") return;

                var msg = JsonConvert.DeserializeObject<WsResponse>(message);
                if (msg.S != null && msg.S == 1)
                {
                    // this is an init message lets confirm
                    SignalR.Start(_httpHost, _connectionToken, _hubs);
                    return;
                }

                // record the related info
                _lastMessageId = msg.C;
                if (!string.IsNullOrEmpty(msg.G))
                    _groupsToken = msg.G;

                // invoke the event
                if (msg.M != null && msg.M.Any() || msg.R != null || !string.IsNullOrEmpty(msg.I) ||
                    !string.IsNullOrEmpty(msg.C))
                    Task.Run(() => { OnNewMessage?.Invoke(msg); });
            }
            catch (Exception e)
            {
                Console.WriteLine($"SignalR Message Error: {e.Message}");
            }
        }

        private void _webSocket_OnFatality(string reason)
        {
            Task.Run(() => { OnFatality?.Invoke(reason); });

            if (Debug)
                WriteLine($"WebSocket Fatality, Reason: {reason}");
        }

        private void _webSocket_OnError(Exception ex)
        {
            Task.Run(() => { OnError?.Invoke(ex); });

            if (Debug)
                WriteLine($"WebSocket Error, Exception: {ex.Message}");
        }

        private void _webSocket_OnData(byte[] data)
        {
            Task.Run(() => { OnData?.Invoke(data); });

            if (Debug)
                WriteLine($"New Data, Data: {Encoding.UTF8.GetString(data)}");
        }

        private void _webSocket_OnClosed(WebSocketCloseStatus reason)
        {
            if (Debug)
                WriteLine($"WebSocket Closed, Reason: {reason}");

            // don't do anything, wait for the monitor to pickup the dropped connection and let it decide what to do
        }

        public int InvokeHubMethod(string hubName, string hubMethod, params object[] parameters)
        {
            if (Debug)
                WriteLine($"Invoking Server Hub Method, Name: {hubName}, Method: {hubMethod}");
            return SignalR.InvokeHubMethod(_webSocket, hubName, hubMethod, parameters);
        }

        /// <summary>
        ///     Sends the data directly over the socket
        /// </summary>
        /// <param name="data">the data to send</param>
        /// <returns></returns>
        public bool Send(string data) => _webSocket.Send(data);

        /// <summary>
        ///     Sends the data as JSON directly over the socket
        /// </summary>
        /// <param name="data">the data to serialize and send</param>
        /// <returns></returns>
        public bool Send(object data) => _webSocket.Send(JsonConvert.SerializeObject(data));

        private void WriteLine(string msg)
        {
            Console.WriteLine($"{DateTime.Now} - {msg}");
        }

        #region IDisposable Support

        private bool _disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                    _webSocket.Dispose(false);

                _disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        #endregion
    }
}