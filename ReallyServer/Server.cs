using SimpleTCPPlus.Server;
using System;
using System.Threading;

namespace ReallyServer
{
    class Server
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Server started");
            var server = new SimpleTcpServer(System.Reflection.Assembly.GetExecutingAssembly()).Start(6124);
            server.ClientConnected += (x, w) =>
            {
                Console.WriteLine($"Client connected [{w.Client.RemoteEndPoint.ToString()}]");
            };
            server.ClientDisconnected += (x, w) =>
            {
                Console.WriteLine($"Client disconnected [{w.Client.RemoteEndPoint.ToString()}]");
            };
            server.DataReceived += (s, pack) =>
            {
                Console.WriteLine($"PACKET:\n{pack.Packet.PacketType}");
                server.PacketHandler(pack);
            };
            new Thread(() => { while (true) { } }).Start();
        }
    }
}
