using System;
using System.Net;
using System.Text;
using PureSignalR.Interfaces;
using PureSignalR.Types;
using PureWebSockets;

namespace PureSignalR
{
    internal static class SignalR
    {
        internal static NegotiateResponse Negotiate(string host, string[] hubs, ISerializer serializer)
        {
            // Transport: http
            // example which should be url encoded before sending
            // negotiate?transport=webSockets&clientProtocol=1.4&connectionData=[{"name":"chathub"},{"name":"notificationhub"}]

            // to simplify things lets just build the url with a simple loop
            var sb = new StringBuilder();
			sb.Append(host).Append("/negotiate?transport=webSockets&clientProtocol=").Append(WebUtility.UrlEncode("1.4")).Append("&connectionData=").Append(WebUtility.UrlEncode("["));
            if (hubs.Length > 0)
            {
	            for (var i = 0; i < hubs.Length; i++)
	            {
		            sb.Append(WebUtility.UrlEncode($"{{\"name\":\"{hubs[i]}\"}}"));
		            if (i < hubs.Length - 1)
			            sb.Append(WebUtility.UrlEncode(","));
	            }
            }

	        sb.Append(WebUtility.UrlEncode("]"));
            var negresp = HttpRequest.Get(sb.ToString(), 15000, true).Result;
            return serializer.Deserialize<NegotiateResponse>(negresp);
        }

        internal static PureWebSocket Connect(string host, string connectionToken, string[] hubs, IPureWebSocketOptions socketOptions)
        {
            // Transport: ws
            // example which should be url encoded before sending
            // connect?transport=webSockets&clientProtocol=1.4&connectionToken=dst4yfTh7tLAxZZtTPhpQh53uon9RDA+Aag56A6XP5xEL4/FADijbZjvoLWtplY+S570UXZwhatkSLQPOBF5RLBRZGoAK4O865XNAoF2ZbOMs03fKcbdirFh8sNSfFqV&connectionData=[{"name":"chathub"},{"name":"notificationhub"}]
            var sb = new StringBuilder();
			sb.Append(host).Append("/connect?transport=webSockets&clientProtocol=").Append(WebUtility.UrlEncode("1.4")).Append("&connectionToken=").Append(WebUtility.UrlEncode(connectionToken)).Append("&connectionData=").Append(WebUtility.UrlEncode("["));
            if (hubs.Length > 0)
			{
				for (var i = 0; i < hubs.Length; i++)
				{
					sb.Append(WebUtility.UrlEncode($"{{\"name\":\"{hubs[i]}\"}}"));
					if (i < hubs.Length - 1)
						sb.Append(WebUtility.UrlEncode(","));
				}
			}

			sb.Append(WebUtility.UrlEncode("]"));

            return new PureWebSocket(sb.ToString(), socketOptions);
        }

        internal static bool Start(string host, string connectionToken, string[] hubs)
        {
            // Transport: http
            // example which should be url encoded before sending
            // start?transport=webSockets&clientProtocol=1.4&connectionToken=LkNk&connectionData=[{"name":"chat"}]
            var sb = new StringBuilder();
			sb.Append(host).Append("/start?transport=webSockets&clientProtocol=").Append(WebUtility.UrlEncode("1.4")).Append("&connectionToken=").Append(WebUtility.UrlEncode(connectionToken)).Append("&connectionData=").Append(WebUtility.UrlEncode("["));
            if (hubs.Length > 0)
			{
				for (var i = 0; i < hubs.Length; i++)
				{
					sb.Append(WebUtility.UrlEncode($"{{\"name\":\"{hubs[i]}\"}}"));
					if (i < hubs.Length - 1)
						sb.Append(WebUtility.UrlEncode(","));
				}
			}

			sb.Append(WebUtility.UrlEncode("]"));

            return HttpRequest.Get(sb.ToString(), 15000, true).Result.IndexOf("started", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        internal static PureWebSocket Reconnect(string host, string connectionToken, string[] hubs, string messageId, string groupsToken, IPureWebSocketOptions socketOptions)
        {
            // Transport: ws
            // example which should be url encoded before sending
            // reconnect?transport=webSockets&clientProtocol=1.4&connectionToken=Aa-aQA&connectionData=[{"Name":"hubConnection"}]&messageId=d-3104A0A8-H,0|L,0|M,2|K,0&groupsToken=AQ
            var sb = new StringBuilder();
			sb.Append(host).Append("/connect?transport=webSockets&clientProtocol=").Append(WebUtility.UrlEncode("1.4")).Append("&connectionToken=").Append(WebUtility.UrlEncode(connectionToken)).Append("&connectionData=").Append(WebUtility.UrlEncode("["));
            if (hubs.Length > 0)
			{
				for (var i = 0; i < hubs.Length; i++)
				{
					sb.Append("{{\"name\":\"").Append(hubs[i]).Append("\"}}");
					if (i < hubs.Length - 1)
						sb.Append(",");
				}
			}

			sb.Append(WebUtility.UrlEncode("]")).Append("&messageId=").Append(WebUtility.UrlEncode(messageId)).Append("&groupsToken=").Append(WebUtility.UrlEncode(groupsToken));

            return new PureWebSocket(sb.ToString(), socketOptions);
        }

        internal static void Abort(string host, string connectionToken, string[] hubs)
        {
            // Transport: http
            // exmaple which should be url encoded before sending
            // abort?transport=longPolling&clientProtocol=1.4&connectionToken=QcnlM&connectionData=[{"name":"chathub"}]
            var sb = new StringBuilder();
			sb.Append(host).Append("/abort?transport=webSockets&clientProtocol=").Append(WebUtility.UrlEncode("1.4")).Append("&connectionToken=").Append(WebUtility.UrlEncode(connectionToken)).Append("&connectionData==").Append(WebUtility.UrlEncode("["));
            if (hubs.Length > 0)
			{
				for (var i = 0; i < hubs.Length; i++)
				{
					sb.Append("{{\"name\":\"").Append(hubs[i]).Append("\"}}");
					if (i < hubs.Length - 1)
						sb.Append(",");
				}
			}

			sb.Append(WebUtility.UrlEncode("]"));

            HttpRequest.Get(sb.ToString(), 10000, true).Wait(12000);
        }

        internal static int InvokeHubMethod(PureWebSocket socket, string hubName, string methodName, ISerializer serializer, params object[] parameters)
        {
            // Transport: ws
            // exmaple which should not be encoded before sending
            // {"H":"chatHub","M":"getOnlineCount","A":[],"I":319104157}
            var msgId = int.Parse(DateTime.Now.ToString("MMddhhmmss"));
            var sb = new StringBuilder();
			sb.Append("{\"H\":\"").Append(hubName).Append("\",\"M\":\"").Append(methodName).Append("\",\"A\":[");
            if (parameters.Length > 0)
			{
				for (var i = 0; i < parameters.Length; i++)
				{
					switch (parameters[i])
					{
						case short _:
						case int _:
						case long _:
						case ushort _:
						case uint _:
						case ulong _:
						case double _:
						case decimal _:
						case float _:
							sb.Append(parameters[i]);
							break;
						case bool _ when (bool)parameters[i]:
							sb.Append(1);
							break;
						case bool _:
							sb.Append(0);
							break;
						case string _:
							sb.Append("\"").Append(parameters[i]).Append("\"");
							break;
						default:
							sb.Append(serializer.Serialize(parameters[i]));
							break;
					}

					if (i < parameters.Length - 1)
						sb.Append(",");
				}
			}
            sb.Append("],\"I\":").Append(msgId).Append("}");
            if (socket.Send(sb.ToString()))
                return msgId;
            return -1;
        }
    }
}