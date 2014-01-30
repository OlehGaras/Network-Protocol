using System.Threading;
using Network_Protocol;
using EndPoint = Network_Protocol.EndPoint;

namespace NetworkProtocolConsoleApplication
{
    class Program
    {
        static void Main()
        {
            var server = new Server(1111);
            EndPoint endPoint = null;
            server.EndpointConnected += (sender, eventArgs) =>
                {
                    endPoint = eventArgs.EndPoint;
                };
            var thread = new Thread(() => server.WaitAndAcceptClient(new CancellationTokenSource().Token, new TestCommandFactory()));
            thread.Start();

            while (endPoint == null)
            {
            }
            thread.Abort();

            endPoint.Start();
            endPoint.Stop();
        }
    }
}
