using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Network_Protocol
{
    public class Server
    {
        private readonly CommandHandler m_Handler = new CommandHandler();
        public int Port { get; private set; }
        private StreamReader m_Reader;
        private StreamWriter m_Writer;
        private readonly CommandFactory m_Factory = new CommandFactory();

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
                        Console.WriteLine("Connected");
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

            string version = string.Empty;
            var line = m_Reader.ReadLine();

            if (line.StartsWith("ProtocolVersion:"))
            {
                version = line.Split(':')[1].Trim();
            }

            if (version == "0.0.0.1")
            {
                m_Writer.WriteLine(line + "Accepted");
                return true;
            }
            return false;
        }

        public void HandleCommands(Stream stream)
        {
            while (true)
            {
                string commandLine = m_Reader.ReadLine();
                var command = m_Factory.Recognize((new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Command>(commandLine)).Id);
                var response = ProcessCommand(command.GetType());
                string json = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(response);
                m_Writer.WriteLine(json);
            }
        }

        public Response ProcessCommand(Type typeOfCommand)
        {
            if (m_Handler.ContainsCommandHandler(typeOfCommand))
            {
                return m_Handler[typeOfCommand].Invoke();
            }
            return null;
        }

    }
}
