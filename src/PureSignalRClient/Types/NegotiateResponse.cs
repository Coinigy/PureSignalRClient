using System.Runtime.Serialization;

namespace PureSignalR.Types
{
    public class NegotiateResponse
    {
		[DataMember(Name = "Url")]
        public string Url { get; set; }

	    [DataMember(Name = "ConnectionToken")]
		public string ConnectionToken { get; set; }

	    [DataMember(Name = "ConnectionId")]
		public string ConnectionId { get; set; }

	    [DataMember(Name = "KeepAliveTimeout")]
		public double KeepAliveTimeout { get; set; }

	    [DataMember(Name = "DisconnectTimeout")]
		public double DisconnectTimeout { get; set; }

	    [DataMember(Name = "ConnectionTimeout")]
		public double ConnectionTimeout { get; set; }

	    [DataMember(Name = "TryWebSockets")]
		public bool TryWebSockets { get; set; }

	    [DataMember(Name = "ProtocolVersion")]
		public string ProtocolVersion { get; set; }

	    [DataMember(Name = "TransportConnectTimeout")]
		public double TransportConnectTimeout { get; set; }

	    [DataMember(Name = "LongPollDelay")]
		public double LongPollDelay { get; set; }
    }
}