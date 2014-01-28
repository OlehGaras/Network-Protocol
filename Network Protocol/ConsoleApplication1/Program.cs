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
            var outClient = new Client().ConnectToServer(IPAddress.Parse("127.0.0.1"), 1111);
            var inClient = new Server(1111).WaitAndAcceptClient(new CancellationTokenSource().Token,new TestCommandFactory());
            var endPoint = new Network_Protocol.EndPoint(inClient, outClient, new TestCommandFactory());

            endPoint.Start();
            endPoint.Stop();
            //var client = new ClientServer().Connect("127.0.0.1", 1111);
            //var endpoint = new ClientServer();
            //endpoint.SendCommands("127.0.0.1", 1111);
            //var helloWorldCommand = new HelloWorldCommand(resp => Console.WriteLine("HelloWorldCommandDone"));
            //helloWorldCommand.WaitHandle.WaitOne();
            //endpoint.StopSendCommand();

            //var client = new Client().ConnectToServer(IPAddress.Parse("127.0.0.1"), 1111);
            //if (client != null)
            //{
            //    var sender = new CommandSender(client, new TestCommandFactory());
            //    var helloWorldCommand = new HelloWorldCommand(resp => Console.WriteLine("HelloWorldCommandDone"));
            //    sender.AddCommand(helloWorldCommand);
            //    sender.StartHandleCommands();
            //    helloWorldCommand.WaitHandle.WaitOne();
            //    sender.StopHandleCommands();
            //}
        }
    }
}
