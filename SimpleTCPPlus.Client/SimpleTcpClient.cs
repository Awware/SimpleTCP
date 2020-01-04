using SimpleTCPPlus.Common;
using SimpleTCPPlus.Common.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleTCPPlus.Client
{
	public class SimpleTcpClient : IDisposable
	{
		public SimpleTcpClient(System.Reflection.Assembly packets)
		{
			ReadLoopIntervalMs = 10;
			TcpClient = new TcpClient();
			PacketLoader = new GlobalPacketLoader(packets);
			ClientPackets = PacketLoader.LoadPackets<IClientPacket>();
		}
		private GlobalPacketLoader PacketLoader { get; } = null;
		private List<IClientPacket> ClientPackets { get; } = null;

		//private List<byte> _queuedMsg = new List<byte>();

		public event EventHandler<PacketWrapper> SpoofReceivedData;
		public event EventHandler<PacketWrapper> DataReceived;
		public event EventHandler<TcpClient> ConnectedToServer;
		public event EventHandler<TcpClient> DisconnectedFromTheServer;

		public string PacketInQue { get; set; } = "";

		private Thread _rxThread;
		internal bool QueueStop { get; set; }
		internal int ReadLoopIntervalMs { get; set; }

		public SimpleTcpClient Connect(string hostNameOrIpAddress, int port)
		{
			if (string.IsNullOrEmpty(hostNameOrIpAddress))
			{
				throw new ArgumentNullException("hostNameOrIpAddress");
			}

			TcpClient.Connect(hostNameOrIpAddress, port);

			if (TcpClient.Connected)
				ConnectedToServer?.Invoke(null, TcpClient);
			StartRxThread();

			return this;
		}

		private void StartRxThread()
		{
			if (_rxThread != null) { return; }

			_rxThread = new Thread(ListenerLoop);
			_rxThread.IsBackground = true;
			_rxThread.Start();
		}

		public SimpleTcpClient Disconnect()
		{
			if (TcpClient == null) { return this; }
			DisconnectedFromTheServer?.Invoke(null, TcpClient);
			TcpClient.Close();
			TcpClient = null;
			return this;
		}

		public TcpClient TcpClient { get; set; } = null;

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

			_rxThread = null;
		}

		private void RunLoopStep()
		{
			if (TcpClient == null) { return; }
			if (TcpClient.Connected == false) { return; }

			int bytesAvailable = TcpClient.Available;
			if (bytesAvailable == 0)
			{
				//Thread.Sleep(10);
				return;
			}

			List<byte> bytesReceived = new List<byte>();

			while (TcpClient.Available > 0 && TcpClient.Connected)
			{
				byte[] nextByte = new byte[1];
				TcpClient.Client.Receive(nextByte, 0, 1, SocketFlags.None);
				bytesReceived.AddRange(nextByte);
			}

			if (bytesReceived.Count > 0)
			{
				NotifyEndTransmissionRx(TcpClient, bytesReceived.ToArray());
				bytesReceived.Clear();
			}
		}

		private void NotifyEndTransmissionRx(TcpClient client, byte[] rawPacket)
		{
			Packet pack = PacketUtils.BytesToPacket(rawPacket);
			if (!string.IsNullOrEmpty(pack.PacketSec))
				pack = SecurityPackets.DecryptPacket(pack);
			PacketWrapper wrap = new PacketWrapper(pack, client);
			if (!string.IsNullOrEmpty(PacketInQue) && wrap.Packet.PacketType == PacketInQue)
				SpoofReceivedData?.Invoke(this, wrap);
			else
				DataReceived?.Invoke(this, wrap);
		}

		public void Write(byte[] data)
		{
			if (TcpClient == null) { throw new Exception("Cannot send data to a null TcpClient (check to see if Connect was called)"); }
			TcpClient.GetStream().Write(data, 0, data.Length);
		}

		public void WritePacket(Packet pack, bool security = true)
		{
			Thread.Sleep(75);
			if(security)
				Write(PacketUtils.PacketToBytes(SecurityPackets.EncryptPacket(pack)));
			else
				Write(PacketUtils.PacketToBytes(pack));
		}
		public PacketWrapper WritePacketAndReceive(Packet pack, string packettype, bool security = true)
		{
			PacketWrapper packet = null;
			PacketInQue = packettype;
			this.SpoofReceivedData += (s, e) => packet = e;
			WritePacket(pack, security);

			Stopwatch sw = new Stopwatch();
			sw.Start();

			while (packet == null) { }

			PacketInQue = "";
			sw.Stop();

			return packet;
		}
		private bool HasPacket(string packetType) => ClientPackets.Where(pack => pack.PacketType == packetType).Count() > 0;
		private IClientPacket GetPacketByPacketType(string type) => ClientPackets.Where(pack => pack.PacketType == type).FirstOrDefault();
		public void PacketHandler(PacketWrapper wrap)
		{
			if (HasPacket(wrap.Packet.PacketType))
			{
				IClientPacket packet = GetPacketByPacketType(wrap.Packet.PacketType);
				packet.Execute(wrap, this);
			}
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects).

				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.
				QueueStop = true;
				if (TcpClient != null)
				{
					try
					{
						TcpClient.Close();
					}
					catch { }
					TcpClient = null;
				}

				disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~SimpleTcpClient() {
		//   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		//   Dispose(false);
		// }

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}
		#endregion
	}
}