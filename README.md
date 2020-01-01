# SimpleTCPPlus
Straightforward and incredibly useful .NET library to handle the repetitive tasks of spinning up and working with TCP sockets (client and server).

![Build Status](https://ci.appveyor.com/api/projects/status/felx0b90mwgr4l4n?svg=true)

Want a TCP server that listens on port 8910 on all the IP addresses on the machine?

```cs
var server = new SimpleTcpServer(Assembly.LoadFrom("PATH TO ASSEMBLY WITH PACKETS")).Start(8910);
```

Want a TCP client that connects to 127.0.0.1 on port 8910?

```cs
var client = new SimpleTcpClient(Assembly.LoadFrom("PATH TO ASSEMBLY WITH PACKETS")).Connect("127.0.0.1", 8910);
```

Want to receive a message event on the server each time you see a newline \n (char 13), and echo back any messages that come in?

```cs
server.Delimiter = 0x13;
server.DelimiterDataReceived += (sender, msg) => {
                msg.ReplyLine("You said: " + msg.MessageString);
            };
```

Want to know how many clients are connected to the server?

```cs
int clientsConnected = server.ConnectedClientsCount;
```

Want to get the IP addresses that the server is listening on?

```cs
var listeningIps = server.GetListeningIPs();
```

Want to get only the IPv4 addresses the server is listening on?

```cs
var listeningV4Ips = server.GetListeningIPs().Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
```
