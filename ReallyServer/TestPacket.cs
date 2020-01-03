using CommonTypess;
using SimpleTCPPlus.Common;
using SimpleTCPPlus.Common.JSON;
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
            wrap.ReplyPacket(new Packet(JsonUtils.SerializeIt(new AnswerType("Test answer", new Random().Next(99999))), "S_ANSWER"));
        }
    }
    public class TestPacket2 : IServerPacket
    {
        public string PacketType => "sasok";

        public void Execute(PacketWrapper wrap, SimpleTcpServer server)
        {
            Console.WriteLine("This test packet!");
            wrap.ReplyPacket(new Packet("", "TEST_REPLY_PACKET"));
        }
    }
}
