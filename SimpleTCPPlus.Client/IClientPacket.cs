using SimpleTCPPlus.Common;

namespace SimpleTCPPlus.Client
{
    public interface IClientPacket
    {
        string PacketType { get; }
        void Execute(PacketWrapper pack, SimpleTcpClient server);
    }
}
