namespace PureSignalRClient.Types
{
    internal class NegotiateResponse
    {
        public string Url { get; set; }
        public string ConnectionToken { get; set; }
        public string ConnectionId { get; set; }
        public double KeepAliveTimeout { get; set; }
        public double DisconnectTimeout { get; set; }
        public double ConnectionTimeout { get; set; }
        public bool TryWebSockets { get; set; }
        public string ProtocolVersion { get; set; }
        public double TransportConnectTimeout { get; set; }
        public double LongPollDelay { get; set; }
    }
}