using System;
using System.Net;
using Network_Protocol;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new Client().ConnectToServer(IPAddress.Parse("127.0.0.1"), 1111);
            if (client != null)
            {
                var sender = new CommandSender(client);
                var helloWorldCommand = new HelloWorldCommand(resp=>Console.WriteLine("HelloWorldCommandDone"));
                sender.AddCommand(helloWorldCommand);
                sender.StartHandleCommands();
                helloWorldCommand.WaitHandle.WaitOne();
                sender.StopHandleCommands();
            }
        }
    }
}
