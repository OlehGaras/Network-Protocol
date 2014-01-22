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
            var streamReader = new StreamReader(stream);
            var streamWriter = new StreamWriter(stream)
            {
                AutoFlush = true
            };

            string version = string.Empty;
            var line = streamReader.ReadLine();

            if (line.StartsWith("ProtocolVersion:"))
            {
                version = line.Split(':')[1].Trim();
            }

            if (version == "0.0.0.1")
            {
                streamWriter.WriteLine(line + Environment.NewLine + "Accepted");
                return true;
            }
            return false;
        }
    }
}
