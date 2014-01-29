using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Network_Protocol;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            var endPoint = new Client().CreateEndpoint(IPAddress.Parse("127.0.0.1"), 1111, new TestCommandFactory());
            endPoint.Start();
            endPoint.Stop();
        }
    }
}
