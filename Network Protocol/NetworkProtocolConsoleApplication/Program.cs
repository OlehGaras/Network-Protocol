using System.Threading;
using Network_Protocol;

namespace NetworkProtocolConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new Server(1111).WaitAndAcceptClient(new CancellationTokenSource().Token);
            var endPoint = new EndPoint(client.GetStream());
            var thread1 = new Thread(endPoint.HandleCommands);
            thread1.Start();        
        }
    }
}
