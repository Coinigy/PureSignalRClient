using System;
using System.IO;
using System.IO.Compression;
using PureSignalR;
using PureSignalR.Types;

namespace PureSignalRClientTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var conn = new PureSignalRClient(new PureSignalRClientOptions()
            {
                DebugMode = false,
                Url = "https://socket.bittrex.com/signalr",
                Hubs = new[] { "c2" }
            });
            conn.OnNewMessage += Conn_NewMessage;
            conn.Connect();

            READ:

            var res = conn.InvokeHubMethod("c2", "subscribeToExchangeDeltas", "BTC-ETH");

            Console.ReadLine();

            goto READ;
        }

        private static void Conn_NewMessage(WsResponse data)
        {
            if (!string.IsNullOrEmpty(data.I))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"{DateTime.Now} ");
                Console.ResetColor();
                Console.Write("- ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"Invocation Succeeded For Id: {data.I} \r\n");
                Console.ResetColor();
                return;
            }
            foreach (var msg in data.M)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"{DateTime.Now} ");
                Console.ResetColor();
                Console.Write("- ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"Hub: {msg.H} ");
                Console.ResetColor();
                Console.Write("- ");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write($"Method: {msg.M} \r\n");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Data: ");
                Console.ResetColor();
                foreach (var a in msg.A)
                {
	                using (var inputStream = new MemoryStream(Convert.FromBase64String((string)a)))
	                using (var gZipStream = new DeflateStream(inputStream, CompressionMode.Decompress))
	                using (var streamReader = new StreamReader(gZipStream))
	                {
		                var decompressed = streamReader.ReadToEnd();

		                Console.Write($"{decompressed} \r\n");
	                }
                }
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("---------------- End Of Message ----------------");
                Console.ResetColor();
            }
        }
    }
}
