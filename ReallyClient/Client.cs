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
            SimpleTcpClient client = new SimpleTcpClient(System.Reflection.Assembly.GetExecutingAssembly());
            client.DataReceived += (s, pack) =>
            {
                Console.WriteLine($"PACKET:\n{pack.Packet.PacketType}");
                client.PacketHandler(pack);
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
            client.WritePacket(new SimpleTCPPlus.Common.Packet("JSON", "INIT", "NULL"));
            new Thread(() => { while (true) { } }).Start();
        }
    }
}
