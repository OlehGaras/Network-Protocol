using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Network_Protocol;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new Client().ConnectToServer(IPAddress.Parse("127.0.0.1"), 1111);
            CommandSender sender = null;
            if (client != null)
            {
                sender = new CommandSender(client.GetStream());
            }
            var thread2 = new Thread(() =>
                {
                    if (sender != null)
                        sender.Execute();
                });
            thread2.Start();
        }
    }
}
