using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Network_Protocol
{
    public class NetProtocol
    {
        private readonly TcpClient m_Client;

        private volatile bool m_IsEnd;
        private readonly StreamReader m_StreamReader;
        private readonly StreamWriter m_StreamWriter;

        public NetProtocol(Stream stream)
        {

            m_StreamReader = new StreamReader(stream);
            m_StreamWriter = new StreamWriter(stream)
                {
                    AutoFlush = true
                };

        }

        public NetProtocol(TcpClient client)
        {
            if (client == null)
                throw new ArgumentNullException("client");
            m_Client = client;

            m_StreamReader = new StreamReader(client.GetStream());
            m_StreamWriter = new StreamWriter(client.GetStream())
                {
                    AutoFlush = true
                };
        }
    }
}
