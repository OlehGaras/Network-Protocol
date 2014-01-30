using System.Net;
using Network_Protocol;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            var endPoint = new Client().CreateEndpoint(IPAddress.Parse("127.0.0.1"), 1111, new TestCommandFactory());
            var endPoint2 = new Client().CreateEndpoint(IPAddress.Parse("127.0.0.1"), 1111, new TestCommandFactory());
            endPoint.Start();
            endPoint2.Start();
            endPoint.Stop();
            endPoint2.Stop();
        }
    }
}
