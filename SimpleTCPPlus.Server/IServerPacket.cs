using SimpleTCPPlus.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTCPPlus.Server
{
    public interface IServerPacket
    {
        string PacketType { get; }
        void Execute(PacketWrapper pack, SimpleTcpServer server);
    }
}
