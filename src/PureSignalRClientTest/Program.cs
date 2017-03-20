using System;
using PureSignalRClient;
using PureSignalRClient.Types;

namespace PureSignalRClientTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var conn = new Client("https://www.cryptopia.co.nz/signalr", "chatHub", "notificationHub")
            {
                //Debug = true
            };
            conn.OnNewMessage += Conn_NewMessage;
            conn.Connect();

            READ:

            Console.ReadLine();

            Console.WriteLine($"Invocation Id: {conn.InvokeHubMethod("chatHub", "getOnlineCount")}");
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
                Console.Write($"Invocation Sucedded For Id: {data.I} \r\n");
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
                    Console.Write($"{a} \r\n");
                }
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("---------------- End Of Message ----------------");
                Console.ResetColor();
            }
        }
    }
}
