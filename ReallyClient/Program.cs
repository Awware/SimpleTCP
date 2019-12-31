using SimpleTCP_Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReallyClient
{
    class Program
    {
        static void Main(string[] args)
        {
            SimpleTcpClient client = new SimpleTcpClient();
            client.Delimiter = 0x61;
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
        }
    }
}
