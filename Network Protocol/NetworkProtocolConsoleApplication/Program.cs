using System.Threading;
using Network_Protocol;

namespace NetworkProtocolConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            var cts = new CancellationTokenSource();


            var server = new Server(1111);
            var client = server.WaitAndAcceptClient(cts.Token);

            var thread1 = new Thread(() =>
                {
                    server.HandleCommands(client.GetStream());
                });
            thread1.Start();        
        }
    }
}
