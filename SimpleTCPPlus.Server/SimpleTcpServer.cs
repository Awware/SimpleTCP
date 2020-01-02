using SimpleTCPPlus.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleTCPPlus.Server
{
    public class SimpleTcpServer
    {
        public SimpleTcpServer(Assembly packets)
        {
            Loader = new GlobalPacketLoader(packets);
            ServerPackets = Loader.LoadPackets<IServerPacket>();
        }

        private GlobalPacketLoader Loader { get; } = null;
        private List<IServerPacket> ServerPackets { get; } = null;
        private List<ServerListener> _listeners = new List<ServerListener>();

        public event EventHandler<TcpClient> ClientConnected;
        public event EventHandler<TcpClient> ClientDisconnected;
        public event EventHandler<PacketWrapper> DataReceived;

        public IEnumerable<IPAddress> GetIPAddresses()
        {
            List<IPAddress> ipAddresses = new List<IPAddress>();

			IEnumerable<NetworkInterface> enabledNetInterfaces = NetworkInterface.GetAllNetworkInterfaces()
				.Where(nic => nic.OperationalStatus == OperationalStatus.Up);
			foreach (NetworkInterface netInterface in enabledNetInterfaces)
            {
                IPInterfaceProperties ipProps = netInterface.GetIPProperties();
                foreach (UnicastIPAddressInformation addr in ipProps.UnicastAddresses)
                {
                    if (!ipAddresses.Contains(addr.Address))
                    {
                        ipAddresses.Add(addr.Address);
                    }
                }
            }

            var ipSorted = ipAddresses.OrderByDescending(ip => RankIpAddress(ip)).ToList();
            return ipSorted;
        }

        public List<IPAddress> GetListeningIPs()
        {
            List<IPAddress> listenIps = new List<IPAddress>();
            foreach (var l in _listeners)
            {
                if (!listenIps.Contains(l.IPAddress))
                {
                    listenIps.Add(l.IPAddress);
                }
            }

            return listenIps.OrderByDescending(ip => RankIpAddress(ip)).ToList();
        }
        
        public void Broadcast(byte[] rawPacket)
        {
            foreach(var client in _listeners.SelectMany(x => x.ConnectedClients))
            {
                client.GetStream().Write(rawPacket, 0, rawPacket.Length);
            }
        }

        public void Broadcast(Packet packet)
        {
            if (packet == null) { return; }
            Broadcast(PacketUtils.PacketToBytes(packet));
        }

        private int RankIpAddress(IPAddress addr)
        {
            int rankScore = 1000;

            if (IPAddress.IsLoopback(addr))
            {
                // rank loopback below others, even though their routing metrics may be better
                rankScore = 300;
            }
            else if (addr.AddressFamily == AddressFamily.InterNetwork)
            {
                rankScore += 100;
                // except...
                if (addr.GetAddressBytes().Take(2).SequenceEqual(new byte[] { 169, 254 }))
                {
                    // APIPA generated address - no router or DHCP server - to the bottom of the pile
                    rankScore = 0;
                }
            }

            if (rankScore > 500)
            {
                foreach (var nic in TryGetCurrentNetworkInterfaces())
                {
                    var ipProps = nic.GetIPProperties();
                    if (ipProps.GatewayAddresses.Any())
                    {
                        if (ipProps.UnicastAddresses.Any(u => u.Address.Equals(addr)))
                        {
                            // if the preferred NIC has multiple addresses, boost all equally
                            // (justifies not bothering to differentiate... IOW YAGNI)
                            rankScore += 1000;
                        }

                        // only considering the first NIC that is UP and has a gateway defined
                        break;
                    }
                }
            }

            return rankScore;
        }

        private static IEnumerable<NetworkInterface> TryGetCurrentNetworkInterfaces()
        {
            try
            {
                return NetworkInterface.GetAllNetworkInterfaces().Where(ni => ni.OperationalStatus == OperationalStatus.Up);
            }
            catch (NetworkInformationException)
            {
                return Enumerable.Empty<NetworkInterface>();
            }
        }

        public SimpleTcpServer Start(int port, bool ignoreNicsWithOccupiedPorts = true)
        {
            var ipSorted = GetIPAddresses();
			bool anyNicFailed = false;
            foreach (var ipAddr in ipSorted)
            {
				try
				{
					Start(ipAddr, port);
				}
				catch (SocketException ex)
				{
					DebugInfo(ex.ToString());
					anyNicFailed = true;
				}
            }

			if (!IsStarted)
				throw new InvalidOperationException("Port was already occupied for all network interfaces");

			if (anyNicFailed && !ignoreNicsWithOccupiedPorts)
			{
				Stop();
				throw new InvalidOperationException("Port was already occupied for one or more network interfaces.");
			}

            return this;
        }

        public SimpleTcpServer Start(int port, AddressFamily addressFamilyFilter)
        {
            var ipSorted = GetIPAddresses().Where(ip => ip.AddressFamily == addressFamilyFilter);
            foreach (var ipAddr in ipSorted)
            {
                try
                {
                    Start(ipAddr, port);
                }
                catch { }
            }

            return this;
        }

		public bool IsStarted { get { return _listeners.Any(l => l.Listener.Active); } }

		public SimpleTcpServer Start(IPAddress ipAddress, int port)
        {
            ServerListener listener = new ServerListener(this, ipAddress, port);
            _listeners.Add(listener);

            return this;
        }

        public void Stop()
        {
			_listeners.All(l => l.QueueStop = true);
			while (_listeners.Any(l => l.Listener.Active)){
				Thread.Sleep(100);
			};
            _listeners.Clear();
        }

        public int ConnectedClientsCount
        {
            get 
            {
                return _listeners.Sum(l => l.ConnectedClientsCount);
            }
        }

        internal void NotifyEndTransmissionRx(byte[] rawPacket, TcpClient client)
        {
            PacketWrapper pack = new PacketWrapper(PacketUtils.BytesToPacket(rawPacket), client);
            DataReceived?.Invoke(this, pack);
        }

        internal void NotifyClientConnected(ServerListener listener, TcpClient newClient)
        {
            ClientConnected?.Invoke(this, newClient);
        }

        internal void NotifyClientDisconnected(ServerListener listener, TcpClient disconnectedClient)
        {
            ClientDisconnected?.Invoke(this, disconnectedClient);
        }

        internal bool HasPacket(string packetType) => ServerPackets.Where(pack => pack.PacketType == packetType).Count() > 0;
        internal IServerPacket GetPacketByPacketType(string type) => ServerPackets.Where(pack => pack.PacketType == type).FirstOrDefault();
        public void PacketHandler(PacketWrapper wrap)
        {
            if (HasPacket(wrap.Packet.PacketType))
            {
                IServerPacket packet = GetPacketByPacketType(wrap.Packet.PacketType);
                packet.Execute(wrap, this);
            }
        }
        #region Debug logging

        [System.Diagnostics.Conditional("DEBUG")]
		void DebugInfo(string format, params object[] args)
		{
			if (_debugInfoTime == null)
			{
				_debugInfoTime = new System.Diagnostics.Stopwatch();
				_debugInfoTime.Start();
			}
			System.Diagnostics.Debug.WriteLine(_debugInfoTime.ElapsedMilliseconds + ": " + format, args);
		}
		System.Diagnostics.Stopwatch _debugInfoTime;

		#endregion Debug logging
	}
}
