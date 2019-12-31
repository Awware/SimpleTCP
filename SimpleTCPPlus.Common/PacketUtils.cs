using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SimpleTCPPlus.Common
{
    public static class PacketUtils
    {
        public static byte[] PacketToBytes(Packet packet)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream memory = new MemoryStream();
            bf.Serialize(memory, packet);
            byte[] bytes = memory.ToArray();
            memory.Close();
            return bytes;
        }
        public static Packet BytesToPacket(byte[] raw)
        {
            return (Packet)new BinaryFormatter().Deserialize(new MemoryStream(raw));
        }
    }
}
