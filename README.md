# PureSignalRClient
**A Cross Platform SignalR Client for .NET Core NetStandard**

**[NuGet Package](https://www.nuget.org/packages/PureSignalRClient)** [![PureSignalRClient](https://img.shields.io/nuget/v/PureSignalRClient.svg)](https://www.nuget.org/packages/PureSignalRClient/) 

##### Requirements
* .NET NetStandard V1.4+

##### Usage
* Example Included in project

        public static void Main(string[] args)
        {
            var conn = new SignalRWsConnection("https://www.cryptopia.co.nz/signalr", "notificationHub")
            {
                //Debug = true
            };
            conn.OnNewMessage += Conn_NewMessage;
            conn.Connect();

            READ:

            Console.ReadLine();

            Console.WriteLine($"Invocation Id: {conn.InvokeHubMethod("notificationHub", "getOnlineCount")}");
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
Provided by: 2017 Coinigy Inc. Coinigy.com
