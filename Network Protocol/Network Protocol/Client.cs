using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Network_Protocol
{
    public class Client
    {
        private readonly CommandSender m_Sender = new CommandSender();
        public TcpClient ConnectToServer(IPAddress ip, int port)
        {
            var client = new TcpClient();
            client.Connect(ip, port);
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
                return client;
            }
            return null;
        }
    }
}
