//using System.Net;
//using System.Net.Sockets;
//using System.Threading;

//namespace Network_Protocol
//{
//    public class ClientServer
//    {
//        private Client m_Client;
//        private Server m_Server;
//        private CommandSender m_Sender;
//        private CommandHandler m_Receiver;

//        public void SendCommands(string ipAddress, int port)
//        {
//            var client = Connect(ipAddress, port);
//            if (client != null)
//            {
//                m_Sender = new CommandSender(client,new TestCommandFactory());
//                m_Sender.StartHandleCommands();
//            }
//        }

//        public void StopSendCommand()
//        {
//            m_Sender.StopHandleCommands();
//        }

//        public void StopHandleCommands()
//        {
//            m_Receiver.StopHandleCommands();
//        }

//        public void HandleCommands(int port, TestCommandFactory factory)
//        {
//            var client = Listen(port);
//            if (client != null)
//            {
//                m_Receiver = new CommandHandler(client, factory);
//                m_Receiver.StartHandleCommand();
//            }
//        }

//        public TcpClient Connect(string ipAddress, int port)
//        {
//            m_Client = new Client();
//            return m_Client.ConnectToServer(IPAddress.Parse(ipAddress), port);
//        }

//        public TcpClient Listen(int port)
//        {            
//            m_Server = new Server(port);
//            return m_Server.WaitAndAcceptClient(new CancellationToken());
//        }
//    }
//}
