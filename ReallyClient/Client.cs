using SimpleTCPPlus.Client;
using SimpleTCPPlus.Common;
using SimpleTCPPlus.Common.Security;
using System;
using System.Threading;

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
            client.ConnectedToServer += (x, clientx) =>
            {
                Console.WriteLine("Connected to the server!");
                client.WritePacket(new Packet("SECURITY DATA", "Security packet"), true);
            };
            client.DisconnectedFromTheServer += (x, c) =>
            {
                Console.WriteLine("Disconnected!");
            };
            while (!client.TcpClient.Connected)
            {
                try
                {
                    client = client.Connect("127.0.0.1", 6124);
                }
                catch { }
            }
            new Thread(() => { while (true) { } }).Start();
        }
    }
}
