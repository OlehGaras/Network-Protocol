using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Network_Protocol
{
    public class Client
    {
        private Guid m_Guid;

        public Client()
        {
            m_Guid = Guid.NewGuid();
        }

        public EndPoint CreateEndpoint(IPAddress ipAddress, int port, CommandFactory commandFactory)
        {
            var firstClient = ConnectToServer(ipAddress, port);
            var secondClient = ConnectToServer(ipAddress, port);
            return new EndPoint(firstClient, secondClient, commandFactory);
        }

        public TcpClient ConnectToServer(IPAddress ip, int port)
        {
            var client = new TcpClient();
            while (!client.Connected)
            {
                try
                {
                    client.Connect(ip, port);
                }
                catch (Exception)
                {}

            }
            var stream = client.GetStream();
            var streamWriter = new StreamWriter(stream)
            {
                AutoFlush = true
            };
            var streamReader = new StreamReader(stream);

            streamWriter.WriteLine(Constants.LineForHandshake);
            var answer = streamReader.ReadLine();
            if (answer != null && answer.Contains(Constants.ServerAnswer))
            {
                streamWriter.WriteLine(m_Guid.ToString());
                answer = streamReader.ReadLine();
                if (answer != null && answer.Contains(Constants.ServerAnswer))
                {
                    return client;
                }
            }
            return null;
        }
    }
}
