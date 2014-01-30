using System.Collections.Generic;
using System.Threading;
using Network_Protocol;

namespace ProxyServerConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new Server(1111);
            var endPoints = new List<EndPoint>();
            var extentions = new List<EndPointExtention>();

            server.EndpointConnected += (sender, eventArgs) => endPoints.Add(eventArgs.EndPoint);
            server.EndPointExtentionCreated += (sender, eventArgs) => extentions.Add(eventArgs.EndPoint); 
            
            var thread = new Thread(() => server.WaitAndAcceptClient(new CancellationTokenSource().Token, new TestCommandFactory()));
            thread.Start();

            while (endPoints.Count < 2)
            {
            }
            thread.Abort();

            var firstEndPoint = endPoints[0];
            var secondEndPoint = endPoints[1];

            Proxy proxy = new Proxy(extentions[0],extentions[1]);
            proxy.Execute();

            firstEndPoint.Start();
            secondEndPoint.Start();

            firstEndPoint.Stop();
            secondEndPoint.Stop();
        }
    }
}
