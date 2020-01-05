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
        public List<TcpClient> ConnectedClients;
        public List<TcpClient> DisconnectedClients;
        private SimpleTcpServer _parent = null;

        public int ConnectedClientsCount => ConnectedClients.Count;

        internal ServerListener(SimpleTcpServer parentServer, IPAddress ipAddress, int port)
        {
            ConnectedClients = new List<TcpClient>();
            DisconnectedClients = new List<TcpClient>();

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
            return !((s.Poll(1000, SelectMode.SelectRead) && (s.Available == 0)) || !s.Connected);
        }

        private void RunLoopStep()
        {
            if (DisconnectedClients.Count > 0)
            {
                var disconnectedClients = DisconnectedClients.ToArray();
                DisconnectedClients.Clear();

                foreach (var disC in disconnectedClients)
                {
                    ConnectedClients.Remove(disC);
                    _parent.NotifyClientDisconnected(this, disC);
                }
            }

            if (Listener.Pending())
            {
                var newClient = Listener.AcceptTcpClient();
                ConnectedClients.Add(newClient);
                _parent.NotifyClientConnected(this, newClient);
            }

            foreach (TcpClient tc in ConnectedClients) {

                if (!IsSocketConnected(tc.Client))
                    DisconnectedClients.Add(tc);

                if (tc.Available == 0)
                    return;

                List<byte> bytesReceived = new List<byte>();

                while (tc.Available > 0 && tc.Connected)
                {
                    byte[] nextByte = new byte[1];
                    tc.Client.Receive(nextByte, 0, 1, SocketFlags.None);
                    bytesReceived.AddRange(nextByte);
                }

                if (bytesReceived.Count > 0)
                {
                    _parent.NotifyEndTransmissionRx(bytesReceived.ToArray(), tc);
                    bytesReceived.Clear();
                }
            }
        }
    }
}
