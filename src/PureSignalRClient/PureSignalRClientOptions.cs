using PureSignalR.Interfaces;
using PureWebSockets;

namespace PureSignalR
{
	public class PureSignalRClientOptions : PureWebSocketOptions
	{
		public string Url { get; set; }
		public string[] Hubs { get; set; }
		public ISerializer Serializer { get; set; }
	}
}
