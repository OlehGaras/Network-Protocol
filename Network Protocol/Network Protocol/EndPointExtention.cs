using System.IO;
using System.Net.Sockets;

namespace Network_Protocol
{
    public class EndPointExtention : EndPoint
    {
        public TcpClient InClient { get; private set; }
        public TcpClient OutClient { get; private set; }

        public EndPointExtention(TcpClient inClient, TcpClient outClient, CommandFactory commandFactory, int keyword) : base(inClient, outClient, commandFactory)
        {
            InClient = inClient;
            OutClient = outClient;
            HandShake(keyword);
        }

        public void HandShake(int keyword)
        {
            var firstStreamWriter = new StreamWriter(InClient.GetStream())
                {
                    AutoFlush = true
                };
            firstStreamWriter.Write(keyword);
            var secondStreamWriter = new StreamWriter(InClient.GetStream())
            {
                AutoFlush = true
            };
            secondStreamWriter.Write(keyword);
        }
    }
}
