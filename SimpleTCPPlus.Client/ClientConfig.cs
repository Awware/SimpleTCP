namespace SimpleTCPPlus.Client
{
    public class ClientConfig
    {
        public string ServerAddress { get; }
        public int ServerPort { get; }
        public ClientConfig(string ip, int port)
        {
            ServerAddress = ip;
            ServerPort = port;
        }
    }
}
