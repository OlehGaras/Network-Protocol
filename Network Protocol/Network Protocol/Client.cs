using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Network_Protocol
{
    public class Client
    {
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
            var lineForHandshake = "ProtocolVersion:0.0.0.1";
            streamWriter.WriteLine(lineForHandshake);
            var answer = streamReader.ReadLine();
            if (answer.Contains("Accepted"))
                return client;
            return null;
        }

        public string PingServer(IPAddress ip, int port)
        {
            var client = new TcpClient();
            client.Connect(ip, port);
            var stream = client.GetStream();
            var streamWriter = new StreamWriter(stream)
            {
                AutoFlush = true
            };
            var streamReader = new StreamReader(stream);
            var lineForHandshake = "ProtocolVersion:0.0.0.1";
            streamWriter.WriteLine(lineForHandshake);
            var answer = streamReader.ReadLine();
            return answer;
        }
    }
}
