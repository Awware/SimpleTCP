using SimpleTCPPlus.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReallyServer
{
    class Server
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Server started");
            var server = new SimpleTcpServer().Start(6124);
            server.DelimiterDataReceived += (s, pack) =>
            {
                Console.WriteLine($"1PACKET:\n{pack.PacketType}");
            };
            server.DataReceived += (s, pack) =>
            {
                Console.WriteLine($"2PACKET:\n{pack.PacketType}");
            };
            new Thread(() => { while (true) { } }).Start();
        }
    }
}
