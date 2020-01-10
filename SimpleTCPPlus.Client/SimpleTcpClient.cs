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
		public ClientConfig Config;
		public SimpleTcpClient(System.Reflection.Assembly packets, ClientConfig cfg)
		{
			Config = cfg;
			//TcpClient = new TcpClient();
			PacketLoader = new GlobalPacketLoader(packets);
			ClientPackets = PacketLoader.LoadPackets<IClientPacket>();
		}
		private GlobalPacketLoader PacketLoader { get; } = null;
		private List<IClientPacket> ClientPackets { get; } = null;

		public event EventHandler<PacketWrapper> SpoofReceivedData;
		public event EventHandler<PacketWrapper> DataReceived;
		public event EventHandler<TcpClient> ConnectedToServer;
		public event EventHandler<TcpClient> DisconnectedFromTheServer;

		public string PacketInQue { get; set; } = "";

		//internal int ReadLoopIntervalMs { get; set; }

		public TcpClient TcpClient { get; set; } = null;

		//public SimpleTcpClient Connect(string hostNameOrIpAddress, int port)
		//{
		//	if (string.IsNullOrEmpty(hostNameOrIpAddress))
		//	{
		//		throw new ArgumentNullException("hostNameOrIpAddress");
		//	}
		//	//TcpClient = new TcpClient();
		//	TcpClient.BeginConnect(hostNameOrIpAddress, port, OnClientConnected, null);
		//	return this;
		//}

		public bool Connect()
		{
			if(TcpClient != null && TcpClient.Connected)
				return true;
			try
			{
				TcpClient = new TcpClient();
				TcpClient.Connect(Config.ServerAddress, Config.ServerPort);
			}
			catch
			{
				if (TcpClient != null && TcpClient.Connected)
					TcpClient.Close();
			}
			if (TcpClient != null && TcpClient.Connected)
			{
				OnClientConnected();
				return true;
			}
			return false;
		}

		private void OnClientConnected()
		{
			//TcpClient client = ar.AsyncState as TcpClient;
			//if (client == null)
			//	return;
			try
			{
				if (!TcpClient.Connected)
					return;

				//TcpClient.EndConnect(ar); //MAY BE BUG!
				ConnectedToServer?.Invoke(null, TcpClient);

				SocketSession travkaLox = new SocketSession(TcpClient);
				travkaLox.OnDataReceived += ClientDataReceived;
				travkaLox.OnSocketException += () =>
				{
					DisconnectedFromTheServer?.Invoke(this, travkaLox.Socket);
					travkaLox.Dispose();
					travkaLox = null;
				};
				travkaLox.BeginRead();
			}
			catch(Exception ex)
			{
				Console.WriteLine($"Error {ex.Message} | {ex.StackTrace}");
			}
		}

		private void ClientDataReceived(byte[] data, TcpClient client)
		{
			NotifyEndTransmissionRx(client, data);
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

		public void Write(byte[] rawPacket)
		{
			if (!TcpClient.Connected)
				return;
			TcpClient.GetStream().Write(rawPacket, 0, rawPacket.Length);
		}

		public void WritePacket(Packet pack, bool security = true)
		{
			//Thread.Sleep(75);
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
			//if (!disposedValue)
			//{
			//	if (disposing)
			//	{
			//		// TODO: dispose managed state (managed objects).

			//	}

			//	// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
			//	// TODO: set large fields to null.
			//	//QueueStop = true;
			//	if (TcpClient != null)
			//	{
			//		try
			//		{
			//			TcpClient.Close();
			//		}
			//		catch { }
			//		TcpClient = null;
			//	}

			//	disposedValue = true;
			//}
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