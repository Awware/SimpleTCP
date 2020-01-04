using SimpleTCPPlus.Common.Security;
using System.Net.Sockets;
using System.Threading;

namespace SimpleTCPPlus.Common
{
    public class PacketWrapper
    {
        public PacketWrapper(Packet pack, TcpClient client) { Packet = pack; Client = client; }
        public Packet Packet { get; } = null; 
        public TcpClient Client { get; } = null;
        public void ReplyPacket(Packet pack, bool security)
        {
            Thread.Sleep(50);
            if (!security)
            {
                byte[] PacketBytes = PacketUtils.PacketToBytes(pack);
                Client.GetStream().Write(PacketBytes, 0, PacketBytes.Length);
            }
            else
            {
                byte[] PacketBytes = PacketUtils.PacketToBytes(SecurityPackets.EncryptPacket(pack));
                Client.GetStream().Write(PacketBytes, 0, PacketBytes.Length);
            }
        }
    }
}
