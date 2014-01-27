using System.Threading;
using Network_Protocol;

namespace NetworkProtocolConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new Server(1111).WaitAndAcceptClient(new CancellationTokenSource().Token);
            var endPoint = new EndPoint(client,new TestCommandFactory());
            endPoint.StartHandleCommand();
            endPoint.StopHandleCommands();           
        }
    }
}
