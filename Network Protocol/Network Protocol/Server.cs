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
        protected StreamReader Reader;
        protected StreamWriter Writer;

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

        public virtual void HandleIncomingClient(TcpClient client, CommandFactory commandFactory)
        {
            var stream = client.GetStream();
            Reader = new StreamReader(stream);
            Writer = new StreamWriter(stream)
                {
                    AutoFlush = true
                };

            var line = Reader.ReadLine();

            if (line != null && String.Compare(line, Constants.LineForHandshake, StringComparison.OrdinalIgnoreCase) == 0)
            {
                Writer.WriteLine(Constants.ServerAnswer);
            }
            else
            {
                client.Close();
                return;
            }
            var guid = Reader.ReadLine();
            if (guid == null)
                return;

            Writer.WriteLine(Constants.ServerAnswer);
            if (m_Guids.ContainsKey(guid))
            {
                OnEndpointConnected(new EndPoint(client, m_Guids[guid], commandFactory));
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

        
    }

    public class ProxyServer : Server
    {
        private EndPoint m_LastCreated;
        private readonly Dictionary<string, EndPoint> m_EndPoints = new Dictionary<string, EndPoint>();

        public ProxyServer(int port)
            : base(port)
        {
        }

        public override void HandleIncomingClient(TcpClient client, CommandFactory commandFactory)
        {
            base.HandleIncomingClient(client, commandFactory);

            var keyword = Reader.ReadLine();
            Writer.WriteLine(Constants.ServerAnswer);

            if (m_LastCreated != null && keyword != null)
            {
                if (m_EndPoints.ContainsKey(keyword))
                {
                    var proxy = new Proxy(m_LastCreated, m_EndPoints[keyword]);
                    OnProxyCreated(proxy);
                    m_EndPoints.Remove(keyword);
                }
                else
                {
                    m_EndPoints.Add(keyword, m_LastCreated);
                    m_LastCreated = null;
                }
            }
        }
        public event EventHandler<ProxyEventArgs> ProxyCreated;
        protected virtual void OnProxyCreated(Proxy e)
        {
            EventHandler<ProxyEventArgs> handler = ProxyCreated;
            if (handler != null) handler(this, new ProxyEventArgs(e));
        }

        protected override void OnEndpointConnected(EndPoint endPoint)
        {
            m_LastCreated = endPoint;
            base.OnEndpointConnected(endPoint);
        }
    }

    public class ProxyEventArgs : EventArgs
    {
        public Proxy Proxy { get; set; }

        public ProxyEventArgs(Proxy proxy)
        {
            Proxy = proxy;
        }
    }

}
