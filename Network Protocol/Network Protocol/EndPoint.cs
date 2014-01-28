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
        private int m_Started = 0;
        private int m_Stoped = 0;

        public EndPoint(TcpClient inClient, TcpClient outClient, CommandFactory commandFactory)
        {
            m_Cts = new CancellationTokenSource();
            m_CommandHandler = new CommandHandler(inClient, commandFactory, m_Cts);
            m_CommandSender = new CommandSender(outClient, commandFactory, m_Cts);
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
                m_Cts.Cancel();
                m_CommandSender.StopHandleCommands();
                m_CommandHandler.StopHandleCommands();
            }
        }
    }
}