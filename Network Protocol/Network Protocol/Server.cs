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
        private bool m_Connected;
        private TcpClient m_Client;


        public Server(int port)
        {
            Port = port;
        }

        public TcpClient WaitAndAcceptClient(CancellationToken token)
        {
            var tcpListener = new TcpListener(IPAddress.Any, Port);
            while (!token.IsCancellationRequested)
            {
                tcpListener.Start();
                if (!tcpListener.Pending())
                {
                }
                else
                {
                    tcpListener.BeginAcceptTcpClient(ClientConnected, tcpListener);
                    if (m_Connected)
                        break;
                }
            }
            tcpListener.Stop();
            return m_Client;
        }

        private void ClientConnected(IAsyncResult ar)
        {
            //var thread = new Thread(()=>new Server(Port + 1).WaitAndAcceptClient(new CancellationToken()));
            //thread.Start();
            var tcpListener = (TcpListener)ar.AsyncState;
            var client = tcpListener.EndAcceptTcpClient(ar);
            var stream = client.GetStream();
            if (HandShake(stream))
            {
                m_Client = client;
                m_Connected = true;
                return;
            }
            client.Close();
            stream.Dispose();
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
