using Newtonsoft.Json;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace tryWebSocket
{
    class Program
    {
        static void Main(string[] args)
        {
            RunWebSockets().GetAwaiter().GetResult();
        }
        private static async Task RunWebSockets()
        {
            var client = new ClientWebSocket();
            await client.ConnectAsync(new Uri("wss://stream.binance.com:9443/ws/!ticker@arr"), CancellationToken.None);
            Console.WriteLine("Connected!");

            var sending = Task.Run(async () =>
            {
                string line;
                while ((line = Console.ReadLine()) != null && line != String.Empty)
                {
                    var bytes = Encoding.UTF8.GetBytes(line);
                    await client.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                }

                await client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            });

            var receiving = Receiving(client);

            await Task.WhenAll(sending, receiving);
        }

        private static async Task Receiving(ClientWebSocket client)
        {
            var buffer = new byte[1024 * 256];

            while (true)
            {
                var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string data = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var x = JsonConvert.DeserializeObject(data);
                    Console.WriteLine(x);
                }

                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                    break;
                }
            }
        }
    }
}
