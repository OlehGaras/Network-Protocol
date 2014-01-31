using System;
using System.Net;
using Network_Protocol;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            var keyword = "keyword";
            var endPoint = new ProxyClient().CreateEndpoint(IPAddress.Parse("127.0.0.1"), 1111, new TestCommandFactory(),keyword);
            var helloWorldCommand = new HelloWorldCommand();
            endPoint.AddCommand(helloWorldCommand);
            endPoint.Start();
            endPoint.AddHandler(helloWorldCommand, Handler);
            helloWorldCommand.WaitHandle.WaitOne();

//            Thread.Sleep(TimeSpan.FromMinutes(1));
            endPoint.Stop();
        }

        private static Response Handler(Command command)
        {
            Console.WriteLine("Hello World");
            return new Response();
        }
    }
}
