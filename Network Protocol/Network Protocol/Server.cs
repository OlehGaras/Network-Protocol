using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Network_Protocol
{
    public class Server
    {
        public int Port { get; private set; }
        private StreamReader m_Reader;
        private StreamWriter m_Writer;
        

        public Server(int port)
        {
            Port = port;           
        }

        public TcpClient WaitAndAcceptClient(CancellationToken token)
        {
            var tcpListener = new TcpListener(IPAddress.Any, Port);
            TcpClient client = null;
            while (!token.IsCancellationRequested)
            {
                tcpListener.Start();
                if (!tcpListener.Pending())
                {
                }
                else
                {
                    client = tcpListener.AcceptTcpClient();
                    var stream = client.GetStream();
                    if (HandShake(stream))
                    {
                        //Console.WriteLine("Connected");
                        break;
                    }
                    client.Close();
                    stream.Dispose();
                    client = null;
                }
            }
            tcpListener.Stop();
            return client;
        }

        public bool HandShake(Stream stream)
        {
            m_Reader = new StreamReader(stream);
            m_Writer = new StreamWriter(stream)
            {
                AutoFlush = true
            };

            var line = m_Reader.ReadLine();

            if (line != null && String.CompareOrdinal(line, Constants.LineForHandshake) == 0)
            {
                m_Writer.WriteLine(Constants.ServerAnswer);
                return true;
            }
            return false;
        }
    }
}
