# SimpleTCPPlus
Straightforward and incredibly useful .NET library to handle the repetitive tasks of spinning up and working with TCP sockets (client and server) + packets.

<b>Want a TCP server that listens on port 8910 on all the IP addresses on the machine?</b>

```cs
var server = new SimpleTcpServer(Assembly.LoadFrom("PATH TO ASSEMBLY WITH PACKETS")).Start(8910);
//or
var server = new SimpleTcpServer(Assembly.GetExecutingAssembly()).Start(8910);
```

<b>Want a TCP client that connects to 127.0.0.1 on port 8910?</b>

```cs
var client = new SimpleTcpClient(Assembly.LoadFrom("PATH TO ASSEMBLY WITH PACKETS")).Connect("127.0.0.1", 8910);
```

<b>Want to receive a message event on the server each time you see a newline \n (char 13), and echo back any messages that come in?</b>

```cs
server.DelimiterDataReceived += (sender, packet) => {
                msg.ReplyLine("You said: " + msg.MessageString);
            };
```

<b>Server</b>
```cs
server.DataReceived += (s, packet) => 
{
      Console.WriteLine($"PACKET:\n{packet.Packet.PacketType}");
      server.PacketHandler(packet); //To work with custom packages that you add.
};
```

<b>Client</b>
```cs
client.DataReceived += (s, packet) => 
{
      Console.WriteLine($"PACKET:\n{packet.Packet.PacketType}");
      client.PacketHandler(packet); //To work with custom packages that you add.
};
```

<b>Want to know how many clients are connected to the server?</b>

```cs
int clientsConnected = server.ConnectedClientsCount;
```

<b>Want to get the IP addresses that the server is listening on?</b>

```cs
var listeningIps = server.GetListeningIPs();
```

<b>Want to get only the IPv4 addresses the server is listening on?</b>

```cs
var listeningV4Ips = server.GetListeningIPs().Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
```

<b>Packets</b>
An example of working with packages is in the directories ReallyServer/ReallyClient
