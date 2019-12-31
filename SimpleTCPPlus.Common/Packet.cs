using System;

namespace SimpleTCPPlus.Common
{
    [Serializable]
    public class Packet
    {
        public Packet(string JSON, string TYPE, string FINGER) { RawJSON = JSON; PacketType = TYPE; Fingerprint = FINGER; }
        public string RawJSON { get; }
        public string PacketType { get; }
        public string Fingerprint { get; }
    }
}
