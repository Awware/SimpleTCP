using SimpleTCPPlus.Common;
using SimpleTCPPlus.Server;
using System;

namespace ReallyServer
{
    public class TestPacket : IServerPacket
    {
        public string PacketType => "INIT";

        public void Execute(PacketWrapper wrap, SimpleTcpServer server)
        {
            Console.WriteLine("This init packet!");
        }
    }
}
