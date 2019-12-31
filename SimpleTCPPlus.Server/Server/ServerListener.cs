using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleTCPPlus.Server
{
    internal class ServerListener
    {
        private List<TcpClient> _connectedClients = new List<TcpClient>();
        private List<TcpClient> _disconnectedClients = new List<TcpClient>();
        private SimpleTcpServer _parent = null;
        private Dictionary<string, List<byte>> _clientBuffers = new Dictionary<string, List<byte>>();
        private byte Delimiter { get; set; }

        public int ConnectedClientsCount
        {
            get { return _connectedClients.Count; }
        }

        public IEnumerable<TcpClient> ConnectedClients { get { return _connectedClients; } }

        internal ServerListener(SimpleTcpServer parentServer, IPAddress ipAddress, int port)
        {
            QueueStop = false;
            _parent = parentServer;
            IPAddress = ipAddress;
            Port = port;
            ReadLoopIntervalMs = 10;

            Listener = new TcpListenerEx(ipAddress, port);
            Listener.Start();

            ThreadPool.QueueUserWorkItem(ListenerLoop);
        }

        internal bool QueueStop { get; set; }
        internal IPAddress IPAddress { get; private set; }
        internal int Port { get; private set; }
        internal int ReadLoopIntervalMs { get; set; }

        internal TcpListenerEx Listener { get; } = null;


		
	    private void ListenerLoop(object state)
        {
            while (!QueueStop)
            {
                try
                {
                    RunLoopStep();
                }
                catch 
                {

                }

                Thread.Sleep(ReadLoopIntervalMs);
            }
            Listener.Stop();
        }

	    
	    private bool IsSocketConnected(Socket s)
	    {
	        // https://stackoverflow.com/questions/2661764/how-to-check-if-a-socket-is-connected-disconnected-in-c
	        bool part1 = s.Poll(1000, SelectMode.SelectRead);
	        bool part2 = (s.Available == 0);
	        if ((part1 && part2) || !s.Connected)
		        return false;
	        else
		        return true;
	    }

        private void RunLoopStep()
        {
            if (_disconnectedClients.Count > 0)
            {
                var disconnectedClients = _disconnectedClients.ToArray();
                _disconnectedClients.Clear();

                foreach (var disC in disconnectedClients)
                {
                    _connectedClients.Remove(disC);
                    _parent.NotifyClientDisconnected(this, disC);
                }
            }

            if (Listener.Pending())
            {
				var newClient = Listener.AcceptTcpClient();
				_connectedClients.Add(newClient);
                _parent.NotifyClientConnected(this, newClient);
            }
            
            Delimiter = _parent.Delimiter;

            foreach (var c in _connectedClients)
            {
		
		        if ( IsSocketConnected(c.Client) == false)
                    _disconnectedClients.Add(c);
		    
                if (c.Available == 0)
                    continue;

                List<byte> bytesReceived = new List<byte>();

                while (c.Available > 0 && c.Connected)
                {
                    byte[] nextByte = new byte[1];
                    c.Client.Receive(nextByte, 0, 1, SocketFlags.None);
                    bytesReceived.AddRange(nextByte);

                    string clientKey = c.Client.RemoteEndPoint.ToString();
                    if (!_clientBuffers.ContainsKey(clientKey))
                        _clientBuffers.Add(clientKey, new List<byte>());

                    List<byte> clientBuffer = _clientBuffers[clientKey];
                    if (nextByte[0] == Delimiter)
                    {
                        byte[] msg = clientBuffer.ToArray();
                        clientBuffer.Clear();
                        _parent.NotifyDelimiterMessageRx(msg);
                    } 
                    else
                        clientBuffer.AddRange(nextByte);
                }

                if (bytesReceived.Count > 0)
                    _parent.NotifyEndTransmissionRx(bytesReceived.ToArray());
            }
        }
    }
}
