using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Network_Protocol
{
    public class Server
    {
        public int Port { get; private set; }
        private readonly Dictionary<string, TcpClient> m_Guids = new Dictionary<string, TcpClient>();

        public Server(int port)
        {
            Port = port;
        }

        public void WaitAndAcceptClient(CancellationToken token, CommandFactory commandFactory)
        {
            var tcpListener = new TcpListener(IPAddress.Any, Port);
            while (!token.IsCancellationRequested)
            {
                tcpListener.Start();
                if (tcpListener.Pending())
                {
                    var client = tcpListener.AcceptTcpClient();
                    HandleIncomingClient(client, commandFactory);
                }
            }
            tcpListener.Stop();
        }

        public void HandleIncomingClient(TcpClient client, CommandFactory commandFactory)
        {
            var stream = client.GetStream();
            var reader = new StreamReader(stream);
            var writer = new StreamWriter(stream)
                {
                    AutoFlush = true
                };

            var line = reader.ReadLine();

            if (line != null && String.Compare(line, Constants.LineForHandshake, StringComparison.OrdinalIgnoreCase) == 0)
            {
                writer.WriteLine(Constants.ServerAnswer);
            }
            else
            {
                client.Close();
                return;
            }
            var guid = reader.ReadLine();
            if (guid == null)
                return;

            writer.WriteLine(Constants.ServerAnswer);
            if (m_Guids.ContainsKey(guid))
            {
                OnEndpointConnected(new EndPoint(client, m_Guids[guid], commandFactory));
                OnEndPointExtentionCreated(new EndPointExtention(client, m_Guids[guid], commandFactory));
                m_Guids.Remove(guid);
            }
            else
            {
                m_Guids.Add(guid, client);
            }
        }

        public event EventHandler<EndpointEventArgs> EndpointConnected;
        protected virtual void OnEndpointConnected(EndPoint endPoint)
        {
            EventHandler<EndpointEventArgs> handler = EndpointConnected;
            if (handler != null)
                handler(this, new EndpointEventArgs(endPoint));
        }

        public event EventHandler<EndPointExtentionEventArgs> EndPointExtentionCreated;

        protected virtual void OnEndPointExtentionCreated(EndPointExtention e)
        {
            EventHandler<EndPointExtentionEventArgs> handler = EndPointExtentionCreated;
            if (handler != null) handler(this, new EndPointExtentionEventArgs(e));
        }
    }
    public class EndPointExtentionEventArgs : EventArgs
    {
        private readonly EndPointExtention m_EndPoint;

        public EndPointExtentionEventArgs(EndPointExtention endPoint)
        {
            m_EndPoint = endPoint;
        }

        public EndPointExtention EndPoint
        {
            get
            {
                if (m_EndPoint != null)
                    return m_EndPoint;
                throw new ArgumentNullException();
            }
        }
    }
}
