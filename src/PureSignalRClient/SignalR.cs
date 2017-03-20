using System;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using PureSignalRClient.Types;
using PureWebSockets;

namespace PureSignalRClient
{
    internal static class SignalR
    {
        internal static NegotiateResponse Negotiate(string host, string[] hubs)
        {
            // Transport: http
            // example which should be url encoded before sending
            // negotiate?transport=webSockets&clientProtocol=1.4&connectionData=[{"name":"chathub"},{"name":"notificationhub"}]

            // to simplify things lets just build the url with a simple loop
            var sb = new StringBuilder();
            sb.Append(
                $"{host}/negotiate?transport=webSockets&clientProtocol={WebUtility.UrlEncode("1.4")}&connectionData={WebUtility.UrlEncode("[")}");
            if (hubs.Length > 0)
                for (var i = 0; i < hubs.Length; i++)
                {
                    sb.Append(WebUtility.UrlEncode($"{{\"name\":\"{hubs[i]}\"}}"));
                    if (i < hubs.Length - 1)
                        sb.Append(WebUtility.UrlEncode(","));
                }
            sb.Append(WebUtility.UrlEncode("]"));
            var negresp = HttpRequest.Get(sb.ToString(), 15000, true).Result;
            return JsonConvert.DeserializeObject<NegotiateResponse>(negresp);
        }

        internal static PureWebSocket Connect(string host, string connectionToken, string[] hubs)
        {
            // Transport: ws
            // example which should be url encoded before sending
            // connect?transport=webSockets&clientProtocol=1.4&connectionToken=dst4yfTh7tLAxZZtTPhpQh53uon9RDA+Aag56A6XP5xEL4/FADijbZjvoLWtplY+S570UXZwhatkSLQPOBF5RLBRZGoAK4O865XNAoF2ZbOMs03fKcbdirFh8sNSfFqV&connectionData=[{"name":"chathub"},{"name":"notificationhub"}]
            var sb = new StringBuilder();
            sb.Append(
                $"{host}/connect?transport=webSockets&clientProtocol={WebUtility.UrlEncode("1.4")}&connectionToken={WebUtility.UrlEncode(connectionToken)}&connectionData={WebUtility.UrlEncode("[")}");
            if (hubs.Length > 0)
                for (var i = 0; i < hubs.Length; i++)
                {
                    sb.Append(WebUtility.UrlEncode($"{{\"name\":\"{hubs[i]}\"}}"));
                    if (i < hubs.Length - 1)
                        sb.Append(WebUtility.UrlEncode(","));
                }
            sb.Append(WebUtility.UrlEncode("]"));

            return new PureWebSocket(sb.ToString(), new ReconnectStrategy(0, 0, 0));
        }

        internal static bool Start(string host, string connectionToken, string[] hubs)
        {
            // Transport: http
            // example which should be url encoded before sending
            // start?transport=webSockets&clientProtocol=1.4&connectionToken=LkNk&connectionData=[{"name":"chat"}]
            var sb = new StringBuilder();
            sb.Append(
                $"{host}/start?transport=webSockets&clientProtocol={WebUtility.UrlEncode("1.4")}&connectionToken={WebUtility.UrlEncode(connectionToken)}&connectionData={WebUtility.UrlEncode("[")}");
            if (hubs.Length > 0)
                for (var i = 0; i < hubs.Length; i++)
                {
                    sb.Append(WebUtility.UrlEncode($"{{\"name\":\"{hubs[i]}\"}}"));
                    if (i < hubs.Length - 1)
                        sb.Append(WebUtility.UrlEncode(","));
                }
            sb.Append(WebUtility.UrlEncode("]"));

            return HttpRequest.Get(sb.ToString(), 15000, true).Result.ToLower().Contains("started");
        }

        internal static PureWebSocket Reconnect(string host, string connectionToken, string[] hubs, string messageId,
            string groupsToken)
        {
            // Transport: ws
            // example which should be url encoded before sending
            // reconnect?transport=webSockets&clientProtocol=1.4&connectionToken=Aa-aQA&connectionData=[{"Name":"hubConnection"}]&messageId=d-3104A0A8-H,0|L,0|M,2|K,0&groupsToken=AQ
            var sb = new StringBuilder();
            sb.Append(
                $"{host}/connect?transport=webSockets&clientProtocol={WebUtility.UrlEncode("1.4")}&connectionToken={WebUtility.UrlEncode(connectionToken)}&connectionData={WebUtility.UrlEncode("[")}");
            if (hubs.Length > 0)
                for (var i = 0; i < hubs.Length; i++)
                {
                    sb.Append($"{{\"name\":\"{hubs[i]}\"}}");
                    if (i < hubs.Length - 1)
                        sb.Append(",");
                }
            sb.Append(
                $"{WebUtility.UrlEncode("]")}&messageId={WebUtility.UrlEncode(messageId)}&groupsToken={WebUtility.UrlEncode(groupsToken)}");

            return new PureWebSocket(sb.ToString(), new ReconnectStrategy(0, 0, 0));
        }

        internal static void Abort(string host, string connectionToken, string[] hubs)
        {
            // Transport: http
            // exmaple which should be url encoded before sending
            // abort?transport=longPolling&clientProtocol=1.4&connectionToken=QcnlM&connectionData=[{"name":"chathub"}]
            var sb = new StringBuilder();
            sb.Append(
                $"{host}/abort?transport=webSockets&clientProtocol={WebUtility.UrlEncode("1.4")}&connectionToken={WebUtility.UrlEncode(connectionToken)}&connectionData=={WebUtility.UrlEncode("[")}");
            if (hubs.Length > 0)
                for (var i = 0; i < hubs.Length; i++)
                {
                    sb.Append($"{{\"name\":\"{hubs[i]}\"}}");
                    if (i < hubs.Length - 1)
                        sb.Append(",");
                }
            sb.Append(WebUtility.UrlEncode("]"));

            HttpRequest.Get(sb.ToString(), 10000, true).Wait(12000);
        }

        internal static int InvokeHubMethod(PureWebSocket socket, string hubName, string methodName,
            params object[] parameters)
        {
            // Transport: ws
            // exmaple which should not be encoded before sending
            // {"H":"chatHub","M":"getOnlineCount","A":[],"I":319104157}
            var msgId = int.Parse(DateTime.Now.ToString("MMddhhmmss"));
            var sb = new StringBuilder();
            sb.Append($"{{\"H\":\"{hubName}\",\"M\":\"{methodName}\",\"A\":[");
            if (parameters.Length > 0)
                for (var i = 0; i < parameters.Length; i++)
                {
                    var cobj = parameters[i];
                    if (cobj is short || cobj is int || cobj is long || cobj is ushort || cobj is uint || cobj is ulong ||
                        cobj is double || cobj is decimal || cobj is float)
                        sb.Append(parameters[i]);
                    else if (cobj is bool)
                        if ((bool) parameters[i])
                            sb.Append(1);
                        else
                            sb.Append(0);
                    else
                        sb.Append($"\"{parameters[i]}\"");
                    if (i < parameters.Length - 1)
                        sb.Append(",");
                }
            sb.Append($"],\"I\":{msgId}}}");

            if (socket.Send(sb.ToString()))
                return msgId;
            return -1;
        }
    }
}