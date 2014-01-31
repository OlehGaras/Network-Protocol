using System;
using System.Net.Sockets;
using System.Threading;

namespace Network_Protocol
{
    public class Proxy
    {
        private const int ChunkSize = 1024 * 4;
        private readonly TcpClient m_FirstInClient;
        private readonly TcpClient m_FirstOutClient;
        private readonly TcpClient m_SecondInClient;
        private readonly TcpClient m_SecondOutClient;
        private Thread m_SendToSecondThread;
        private Thread m_SendToFirstThread;
        private CancellationTokenSource  m_Cts = new CancellationTokenSource();

        public Proxy(EndPoint first, EndPoint second)
            : this(first.InClient, first.OutClient, second.InClient, second.OutClient)
        {
        }

        public Proxy(TcpClient firstInClient, TcpClient firstOutClient, TcpClient secondInClient,
                     TcpClient secondOutClient)
        {
            m_FirstInClient = firstInClient;
            m_FirstOutClient = firstOutClient;
            m_SecondInClient = secondInClient;
            m_SecondOutClient = secondOutClient;
        }

        public void Execute()
        {
            m_SendToSecondThread = new Thread(() => SendFromTo(m_FirstInClient, m_SecondOutClient));
            m_SendToSecondThread.Start();

            m_SendToFirstThread = new Thread(() => SendFromTo(m_FirstOutClient, m_SecondInClient));
            m_SendToFirstThread.Start();

            m_SendToFirstThread.Join();
            m_SendToSecondThread.Join();
        }

        public void StopExecute()
        {
            m_Cts.Cancel();
        }

        public void SendFromTo(TcpClient firstClient, TcpClient secondClient)
        {
            var token = m_Cts.Token;
            var buffer = new byte[ChunkSize];

            var firstStream = firstClient.GetStream();
            var secondStream = secondClient.GetStream();

            DateTime lastReadTime = DateTime.Now;

            while (firstClient.Connected && secondClient.Connected && !token.IsCancellationRequested && (DateTime.Now - lastReadTime) < TimeSpan.FromSeconds(500))
            {
                while (firstClient.Client.Poll(200, SelectMode.SelectRead) && firstStream.DataAvailable)
                {
                    try
                    {
                        var bytes = firstStream.Read(buffer, 0, ChunkSize);
                        secondStream.Write(buffer, 0, bytes);
                        secondStream.Flush();
                        lastReadTime = DateTime.Now;
                    }
                    catch (Exception)
                    {
                    }
                }

                while (secondClient.Client.Poll(200, SelectMode.SelectRead) && secondStream.DataAvailable)
                {
                    try
                    {
                        var bytes = secondStream.Read(buffer, 0, ChunkSize);
                        firstStream.Write(buffer, 0, bytes);
                        firstStream.Flush();
                        lastReadTime = DateTime.Now;
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            firstClient.Close();
            secondClient.Close();
        }
    }
}
