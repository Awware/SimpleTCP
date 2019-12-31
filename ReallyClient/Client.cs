using SimpleTCPPlus.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReallyClient
{
    class Client
    {
        static void Main(string[] args)
        {
            SimpleTcpClient client = new SimpleTcpClient();
            client.Delimiter = 0x61;
            client.DelimiterDataReceived += (s, msg) =>
            {
                Console.WriteLine(msg.MessageString);
            };
            while (!client.TcpClient.Connected)
            {
                try
                {
                    client = client.Connect("127.0.0.1", 6124);
                }
                catch { Console.WriteLine("Reconnecting..."); }
            }
            Console.WriteLine("Connected!");
            var replyMsg = client.WriteLineAndGetReply("Hello!", TimeSpan.FromSeconds(3));
            new Thread(() => { while (true) { } }).Start();
        }
    }
}
