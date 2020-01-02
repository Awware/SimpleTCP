using CommonTypes;
using CommonTypess;
using SimpleTCPPlus.Common;
using SimpleTCPPlus.Common.JSON;
using SimpleTCPPlus.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReallyClient
{
    public class FileUpload : IServerPacket
    {
        public string PacketType => "UPL";

        public void Execute(PacketWrapper pack, SimpleTcpServer server)
        {
            FileUploadType type = JsonUtils.DeserializeIt<FileUploadType>(pack.Packet.RawData);
            File.WriteAllBytes(type.Name, type.File);
            pack.ReplyPacket(new Packet(JsonUtils.SerializeIt(new AnswerType("Test answer", new Random().Next(99999))), "S_ANSWER"));
        }
    }
}
