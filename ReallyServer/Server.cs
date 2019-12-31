using SimpleTCPPlus_Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReallyServer
{
    class Server
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Server started");
            var server = new SimpleTcpServer().Start(6124);
            server.Delimiter = 0x61;
            server.DelimiterDataReceived += (s, msg) =>
            {
                msg.ReplyLine(msg.MessageString);
                Console.WriteLine($"You said : {msg.MessageString}");
            };
            new Thread(() => { while (true) { } }).Start();
        }
    }
}
