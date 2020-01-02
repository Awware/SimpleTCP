using System;
using System.Text;

namespace SimpleTCPPlus.Common
{
    [Serializable]
    public class Packet
    {
        public Packet(string Data, string TYPE) { RawData = Encoding.Default.GetBytes(Data); PacketType = TYPE; }
        public Packet(byte[] Data, string TYPE/*, string FINGER*/) { RawData = Data; PacketType = TYPE; /*Fingerprint = FINGER; */}
        public byte[] RawData { get; }
        public string PacketType { get; }
        //public string Fingerprint { get; }
    }
}
