using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Network_Protocol;

namespace NetworkProtocolConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            var inClient = new Server(1111).WaitAndAcceptClient(new CancellationTokenSource().Token,new TestCommandFactory());
            var outClient = new Client().ConnectToServer(IPAddress.Parse("127.0.0.1"), 1111);
            var endPoint = new Network_Protocol.EndPoint(inClient, outClient, new TestCommandFactory());

            endPoint.Start();
            endPoint.Stop();

            //var endpoint = new ClientServer();
            //endpoint.HandleCommands(1111,new TestCommandFactory());
            //endpoint.StopHandleCommands();

            //var client = new Server(1111).WaitAndAcceptClient(new CancellationTokenSource().Token);
            //var endPoint = new CommandHandler(client, new TestCommandFactory());
            //endPoint.StartHandleCommand();
            //endPoint.StopHandleCommands();           
        }
    }
}
