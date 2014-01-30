using System;
using System.Net.Sockets;

namespace Network_Protocol
{
    public class EndPointExtention : EndPoint
    {
        public TcpClient InClient { get; private set; }
        public TcpClient OutClient { get; private set; }
        public Guid Guid { get; private set; }

        public EndPointExtention(TcpClient inClient, TcpClient outClient, CommandFactory commandFactory) : base(inClient, outClient, commandFactory)
        {
            InClient = inClient;
            OutClient = outClient;
            Guid = Guid.NewGuid();
        }
    }
}
