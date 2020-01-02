using System;

namespace SimpleTCPPlus.Common
{
    [Serializable]
    public class Packet
    {
        public Packet(string Data, string TYPE/*, string FINGER*/) { RawData = Data; PacketType = TYPE; /*Fingerprint = FINGER; */}
        public string RawData { get; }
        public string PacketType { get; }
        //public string Fingerprint { get; }
    }
}
