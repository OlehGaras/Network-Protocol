using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Network_Protocol
{
    public class Client
    {
        private Guid m_Guid;
        protected StreamWriter Writer;
        protected StreamReader Reader;

        public Client()
        {
            m_Guid = Guid.NewGuid();
        }

        public EndPoint CreateEndpoint(IPAddress ipAddress, int port, CommandFactory commandFactory, string guid)
        {
            var firstClient = ConnectToServer(ipAddress, port, guid);
            var secondClient = ConnectToServer(ipAddress, port, guid);
            return new EndPoint(firstClient, secondClient, commandFactory);
        }

        public virtual TcpClient ConnectToServer(IPAddress ip, int port, string keyword)
        {
            var client = new TcpClient();
            while (!client.Connected)
            {
                try
                {
                    client.Connect(ip, port);
                }
                catch (Exception)
                { }

            }
            var stream = client.GetStream();
            Writer = new StreamWriter(stream)
            {
                AutoFlush = true
            };
            Reader = new StreamReader(stream);

            Writer.WriteLine(Constants.LineForHandshake);
            var answer = Reader.ReadLine();
            if (answer != null && answer.Contains(Constants.ServerAnswer))
            {
                Writer.WriteLine(m_Guid.ToString());
                answer = Reader.ReadLine();
                if (answer == null || !answer.Contains(Constants.ServerAnswer))
                    return null;
                return client;
            }
            return null;
        }
    }

    public class ProxyClient : Client
    {
        public override TcpClient ConnectToServer(IPAddress ip, int port, string keyword)
        {
            var client  = base.ConnectToServer(ip, port, keyword);
            if (client != null)
            {
                Writer.WriteLine(keyword);
                var answer = Reader.ReadLine();
                if (answer == null || !answer.Contains(Constants.ServerAnswer))
                    return null;               
            }
            return client;
        }
    }
}
