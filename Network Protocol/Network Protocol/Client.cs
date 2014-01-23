using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Network_Protocol
{
    public class Client
    {
        private const string LineForHandshake = "ProtocolVersion:0.0.0.1";
        private const string ServerAnswer = "Accepted";
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

            streamWriter.WriteLine(LineForHandshake);
            var answer = streamReader.ReadLine();

            if (answer.Contains(ServerAnswer))
            {
                m_Sender.AddCommand(new SomeCommand(), () => { Console.WriteLine("SomeCommand Done!!!"); });
                m_Sender.Send(streamWriter,streamReader);
                return client;
            }
            return null;
        }
    }
}
