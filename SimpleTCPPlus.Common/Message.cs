using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace SimpleTCPPlus.Common
{
    //Integrate packets
    public class Message
    {
        private Encoding _encoder = null;
        private byte _writeLineDelimiter;
        private bool _autoTrim = false;

        public TcpClient TcpClient { get; } = null;
        public Message(byte[] data, TcpClient tcpClient, Encoding stringEncoder, byte lineDelimiter)
        {
            Data = data;
            TcpClient = tcpClient;
            _encoder = stringEncoder;
            _writeLineDelimiter = lineDelimiter;
        }

        public Message(byte[] data, TcpClient tcpClient, Encoding stringEncoder, byte lineDelimiter, bool autoTrim)
        {
            Data = data;
            TcpClient = tcpClient;
            _encoder = stringEncoder;
            _writeLineDelimiter = lineDelimiter;
            _autoTrim = autoTrim;
        }

        public byte[] Data { get; private set; }
        public string MessageString
        {
            get
            {
                if (_autoTrim)
                    return _encoder.GetString(Data).Trim();

                return _encoder.GetString(Data);
            }
        }

        public void Reply(byte[] data)
        {
            TcpClient.GetStream().Write(data, 0, data.Length);
        }

        public void Reply(string data)
        {
            if (string.IsNullOrEmpty(data)) { return; }
            Reply(_encoder.GetBytes(data));
        }

        public void ReplyLine(string data)
        {
            if (string.IsNullOrEmpty(data)) { return; }
            if (data.LastOrDefault() != _writeLineDelimiter)
                Reply(data + _encoder.GetString(new byte[] { _writeLineDelimiter }));
            else
                Reply(data);
        }
    }
}