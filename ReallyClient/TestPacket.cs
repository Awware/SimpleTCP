using CommonTypess;
using SimpleTCPPlus.Client;
using SimpleTCPPlus.Common;
using SimpleTCPPlus.Common.JSON;

namespace ReallyClient
{
    public class TestPacket : IClientPacket
    {
        public string PacketType => "S_ANSWER";

        public void Execute(PacketWrapper pack, SimpleTcpClient server)
        {
            System.Console.WriteLine("Server answer received!");
            AnswerType type = JsonUtils.DeserializeIt<AnswerType>(pack.Packet.RawData);
            System.Console.WriteLine($"Answer info : '{type.SomeData} | {type.AnswerInt}'");
        }
    }
}
