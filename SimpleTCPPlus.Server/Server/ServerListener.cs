using SimpleTCPPlus.Common;
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
        public List<TcpClient> DisconnectPool;
        private SimpleTcpServer _parent = null;
        public int ConnectedClientsCount => ConnectedClients.Count;
        internal bool QueueStop { get; set; }
        internal IPAddress IPAddress { get; private set; }
        internal int Port { get; private set; }
        internal int ReadLoopIntervalMs { get; set; }

        internal TcpListenerEx Listener { get; } = null;

        internal ServerListener(SimpleTcpServer parentServer, IPAddress ipAddress, int port)
        {
            ConnectedClients = new List<TcpClient>();
            DisconnectPool = new List<TcpClient>();

            QueueStop = false;
            _parent = parentServer;
            IPAddress = ipAddress;
            Port = port;
            ReadLoopIntervalMs = 10;

            Listener = new TcpListenerEx(ipAddress, port);

            ThreadPool.QueueUserWorkItem(ListenerLoop);
        }


		
	    private void ListenerLoop(object state)
        {
            Listener.Start();
            Listener.BeginAcceptTcpClient(OnClientAccepted, Listener);
        }

        private void StopListener()
        {
            //Close all sockets//
            Listener.Stop();
        }

        private void OnClientAccepted(IAsyncResult ar)
        {
            TcpListenerEx listener = ar.AsyncState as TcpListenerEx;
            if (listener == null)
                return;
            try
            {
                SocketSession session = new SocketSession(listener.EndAcceptTcpClient(ar));
                session.OnDataReceived += (s, x) => _parent.NotifyEndTransmissionRx(s, x);
                session.OnSocketException += () =>
                {
                    _parent.NotifyClientDisconnected(this, session.Socket);
                    session.Socket.Dispose();
                    session = null;
                };
                ConnectedClients.Add(session.Socket);
                _parent.NotifyClientConnected(this, session.Socket);
                session.BeginRead();
            }
            catch { }
            finally
            {
                listener.BeginAcceptTcpClient(OnClientAccepted, listener);
            }
        }
        //private void OnClientRead(IAsyncResult ar)
        //{
        //    StateObject obj = ar.AsyncState as StateObject;
        //    if (obj == null)
        //        return;
        //    try
        //    {
        //        if (!obj.WorkClient.Connected)
        //            return;
        //        int read = obj.WorkClient.Client.EndReceive(ar);

        //        if (read > 0 && obj.WorkClient.Connected)
        //        {
        //            obj.Message.Write(obj.buffer, 0, read);
        //            obj.WorkClient.Client.BeginReceive(obj.buffer, 0, obj.buffer.Length, 0, OnClientRead, obj);
        //        }
        //        else
        //            OnMessageReceived(obj);
        //    }
        //    catch(Exception ex)
        //    {
        //        Console.WriteLine($"{ex.Message}\n{ex.StackTrace}");
        //        _parent.NotifyClientDisconnected(this, obj.WorkClient);
        //        obj.WorkClient.Close();
        //        obj.Message.Close();
        //        obj = null;
        //    }
        //    //finally
        //    //{
        //    //    if (obj != null)
        //    //        obj.WorkClient.Client.BeginReceive(obj.buffer, 0, obj.buffer.Length, 0, OnClientRead, obj);
        //    //}
        //}
        //private void OnMessageReceived(StateObject ctx)
        //{
        //    _parent.NotifyEndTransmissionRx(ctx.Message.ToArray(), ctx.WorkClient);
        //}
    }
}
