using SimpleTCP_Server.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReallyServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new SimpleTcpServer().Start(6124);
            server.Delimiter = 0x61;
            server.DelimiterDataReceived += (s, msg) =>
            {
                msg.ReplyLine($"You said : {msg.MessageString}");
            };
        }
    }
}
