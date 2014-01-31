using System.Collections.Generic;
using System.Threading;
using Network_Protocol;

namespace ProxyServerConsoleApplication
{
    class Program
    {
        private static object m_SyncObject = new object();
        static void Main(string[] args)
        {
            var server = new ProxyServer(1111);
            var endPoints = new List<EndPoint>();
            Proxy proxy = null;

            server.EndpointConnected += (sender, eventArgs) =>
                {
                    lock (m_SyncObject)
                    {
                        endPoints.Add(eventArgs.EndPoint);
                    }
                };
            server.ProxyCreated += (sender, eventArgs) =>
                {
                    proxy = eventArgs.Proxy;
                };
            
            var thread = new Thread(() => server.WaitAndAcceptClient(new CancellationTokenSource().Token, new TestCommandFactory()));
            thread.Start();

            while (proxy == null)
            {
            }
            thread.Abort();
            proxy.Execute();
        }
    }
}
