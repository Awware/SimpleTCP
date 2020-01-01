using System.Net.Sockets;

namespace SimpleTCPPlus.Common
{
    public class PacketWrapper
    {
        public PacketWrapper(Packet pack, TcpClient client) { Packet = pack; Client = client; }
        public Packet Packet { get; } = null; 
        public TcpClient Client { get; } = null;
        public void ReplyPacket(Packet pack)
        {
            Client.GetStream().Write(PacketUtils.PacketToBytes(pack), 0, PacketUtils.PacketToBytes(pack).Length);
        }
    }
}
