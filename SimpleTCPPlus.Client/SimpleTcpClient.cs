using SimpleTCPPlus.Common;
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
			StringEncoder = System.Text.Encoding.UTF8;
			ReadLoopIntervalMs = 10;
			TcpClient = new TcpClient();
			PacketLoader = new GlobalPacketLoader(packets);
			ClientPackets = PacketLoader.LoadPackets<IClientPacket>();
		}
		private GlobalPacketLoader PacketLoader { get; } = null;
		private List<IClientPacket> ClientPackets { get; } = null;

		private List<byte> _queuedMsg = new List<byte>();
		public byte Delimiter { get; } = 0x13;
		public Encoding StringEncoder { get; set; }

		public event EventHandler<PacketWrapper> DelimiterDataReceived;
		public event EventHandler<PacketWrapper> DataReceived;

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
				Thread.Sleep(10);
				return;
			}

			List<byte> bytesReceived = new List<byte>();

			while (TcpClient.Available > 0 && TcpClient.Connected)
			{
				byte[] nextByte = new byte[1];
				TcpClient.Client.Receive(nextByte, 0, 1, SocketFlags.None);
				bytesReceived.AddRange(nextByte);
				if (nextByte[0] == Delimiter)
				{
					byte[] msg = _queuedMsg.ToArray();
					_queuedMsg.Clear();
					NotifyDelimiterMessageRx(TcpClient, msg);
				}
				else
					_queuedMsg.AddRange(nextByte);
			}

			if (bytesReceived.Count > 0)
				NotifyEndTransmissionRx(TcpClient, bytesReceived.ToArray());
		}

		private void NotifyDelimiterMessageRx(TcpClient client, byte[] rawPacket)
		{
			//Message m = new Message(msg, client, StringEncoder, Delimiter, AutoTrimStrings);
			PacketWrapper pack = new PacketWrapper(PacketUtils.BytesToPacket(rawPacket), client);
			DelimiterDataReceived?.Invoke(this, pack);
		}

		private void NotifyEndTransmissionRx(TcpClient client, byte[] rawPacket)
		{
			PacketWrapper pack = new PacketWrapper(PacketUtils.BytesToPacket(rawPacket), client);
			DataReceived?.Invoke(this, pack);
		}

		public void Write(byte[] data)
		{
			if (TcpClient == null) { throw new Exception("Cannot send data to a null TcpClient (check to see if Connect was called)"); }
			TcpClient.GetStream().Write(data, 0, data.Length);
		}

		//public void Write(string data)
		//{
		//	if (data == null) { return; }
		//	Write(StringEncoder.GetBytes(data));
		//}

		//public void WriteLine(string data)
		//{
		//	if (string.IsNullOrEmpty(data)) { return; }
		//	if (data.LastOrDefault() != Delimiter)
		//		Write(data + StringEncoder.GetString(new byte[] { Delimiter }));
		//	else
		//		Write(data);
		//}

		//public Message WriteLineAndGetReply(string data, TimeSpan timeout)
		//{
		//	Message mReply = null;
		//	this.DataReceived += (s, e) => { mReply = e; };
		//	WriteLine(data);

		//	Stopwatch sw = new Stopwatch();
		//	sw.Start();

		//	while (mReply == null && sw.Elapsed < timeout)
		//	{
		//		Thread.Sleep(10);
		//	}

		//	sw.Stop();

		//	return mReply;
		//}

		public void WritePacket(Packet pack)
		{
			Write(AddByteToBytes(PacketUtils.PacketToBytes(pack), Delimiter));
		}
		private byte[] AddByteToBytes(byte[] array, byte b)
		{
			byte[] newArray = new byte[array.Length + 1];
			array.CopyTo(newArray, 1);
			newArray[0] = b;
			return newArray;
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