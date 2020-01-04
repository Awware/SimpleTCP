using CommonTypes;
using CommonTypess;
using SimpleTCPPlus.Client;
using SimpleTCPPlus.Common;
using SimpleTCPPlus.Common.JSON;
using System.IO;
using System.Text;

namespace ReallyClient
{
    public class TestPacket : IClientPacket
    {
        public string PacketType => "S_ANSWER";

        public void Execute(PacketWrapper pack, SimpleTcpClient client)
        {
            System.Console.WriteLine("Server answer received!");
            AnswerType type = JsonUtils.DeserializeIt<AnswerType>(pack.Packet.RawData);
            System.Console.WriteLine($"Answer info : '{type.SomeData} | {type.AnswerInt}'");
        }
    }
    public class SecurityPacket : IClientPacket
    {
        public string PacketType => "Security packet";

        public void Execute(PacketWrapper pack, SimpleTcpClient client)
        {
            System.Console.WriteLine("Counter security packet />/");
            System.Console.WriteLine($"Security data : {pack.Packet.PacketSec} | {pack.Packet.PacketType} | {Encoding.Default.GetString(pack.Packet.RawData)}");
        }
    }
}
