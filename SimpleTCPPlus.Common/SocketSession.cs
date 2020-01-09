using System;
using System.Net.Sockets;

namespace SimpleTCPPlus.Common
{
    public class SocketSession : IDisposable
    {
        public NetworkStream Network;
        public TcpClient Socket;
        public byte[] Buffer = new byte[8192];
        public delegate void DelegateSocketException();
        public event DelegateSocketException OnSocketException;
        public delegate void DelegateDataReceived(byte[] data, TcpClient tcp);
        public event DelegateDataReceived OnDataReceived;
        public SocketSession(TcpClient client)
        {
            Socket = client;
            Network = Socket.GetStream();
        }
        public void BeginRead()
        {
            Network.BeginRead(Buffer, 0, Buffer.Length, OnRead, null);
        }

        private void OnRead(IAsyncResult ar)
        {
            try
            {
                int end = Network.EndRead(ar);
                if (end > 0 && Socket.Connected)
                    OnDataReceived?.Invoke(Buffer, Socket);
                BeginRead();
            }
            catch { OnSocketException?.Invoke(); }
        }

        public void Dispose()
        {
            Network.Close();
            Socket.Close();
        }
    }
}
