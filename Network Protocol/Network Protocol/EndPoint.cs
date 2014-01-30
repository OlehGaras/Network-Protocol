using System;
using System.Net.Sockets;
using System.Threading;

namespace Network_Protocol
{
    public class EndPoint
    {
        private readonly CommandSender m_CommandSender;
        private readonly CommandHandler m_CommandHandler;
        private readonly CancellationTokenSource m_Cts;
        private int m_Started;
        private int m_Stoped;

        public EndPoint(TcpClient inClient, TcpClient outClient, CommandFactory commandFactory)
        {
            if (inClient == null) 
                throw new ArgumentNullException("inClient");
            if (outClient == null) 
                throw new ArgumentNullException("outClient");
            m_Cts = new CancellationTokenSource();
            m_CommandHandler = new CommandHandler(inClient, commandFactory, m_Cts);
            m_CommandSender = new CommandSender(outClient, commandFactory, m_Cts);
            m_CommandHandler.CloseCommandHandled += (sender, args) => Stop();
            m_CommandSender.ConnectionLost += (sender, args) => Stop();
        }

        public void Start()
        {
            if (Interlocked.Increment(ref m_Started) == 1)
            {
                m_CommandHandler.StartHandleCommand();
                m_CommandSender.StartHandleCommands();
            }
        }

        public void Stop()
        {
            if (Interlocked.Increment(ref m_Stoped) == 1)
            {
                if (m_Started == 0)
                {
                    Interlocked.Decrement(ref m_Stoped);
                    return;
                }
                m_CommandSender.StopHandleCommands();
                m_CommandHandler.StopHandleCommands();
            }
        }

        public void AddCommand(Command command)
        {
            m_CommandSender.AddCommand(command);
        }

        public void AddHandler(Command command, Handler handler)
        {
            m_CommandHandler.AddHandler(command.GetType(), handler);
        }
    }
}