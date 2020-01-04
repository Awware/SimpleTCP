using CommonTypess;
using SimpleTCPPlus.Common;
using SimpleTCPPlus.Common.JSON;
using SimpleTCPPlus.Common.Security;
using SimpleTCPPlus.Server;
using System;
using System.Text;

namespace ReallyServer
{
    public class TestPacket : IServerPacket
    {
        public string PacketType => "INIT";

        public void Execute(PacketWrapper wrap, SimpleTcpServer server)
        {
            Console.WriteLine("This init packet!");
            wrap.ReplyPacket(new Packet(JsonUtils.SerializeIt(new AnswerType("Test answer", new Random().Next(99999))), "S_ANSWER"), false);
        }
    }
    public class SecurityPacket : IServerPacket
    {
        public string PacketType => "Security packet";

        public void Execute(PacketWrapper wrap, SimpleTcpServer server)
        {
            Console.WriteLine("Security packet received!");
            Console.WriteLine($"Security data : {wrap.Packet.PacketSec} | {wrap.Packet.PacketType} | {Encoding.Default.GetString(wrap.Packet.RawData)}");
            wrap.ReplyPacket(new Packet("Counter packet", "Security packet"), true);
        }
    }
}
