using SimpleTCPPlus.Client;
using SimpleTCPPlus.Common;

namespace ReallyClient
{
    public class TestPacket : IClientPacket
    {
        public string PacketType => "S_ANSWER";

        public void Execute(PacketWrapper pack, SimpleTcpClient server)
        {
            System.Console.WriteLine("Server answer received!");
        }
    }
}
