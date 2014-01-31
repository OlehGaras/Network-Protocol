using System;
using System.Net.Sockets;
using System.Threading;

namespace Network_Protocol
{
    public class Proxy
    {       
        private readonly TcpClient m_FirstInClient;
        private readonly TcpClient m_FirstOutClient;
        private readonly TcpClient m_SecondInClient;
        private readonly TcpClient m_SecondOutClient;
        private Thread m_SendToSecondThread;
        private Thread m_SendToFirstThread;
        private readonly CancellationTokenSource  m_Cts = new CancellationTokenSource();

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
            DateTime lastReadTime = DateTime.Now;
            var token = m_Cts.Token;
            
            var firstStream = firstClient.GetStream();
            var secondStream = secondClient.GetStream();           

            while (firstClient.Connected && secondClient.Connected && !token.IsCancellationRequested && (DateTime.Now - lastReadTime) < TimeSpan.FromSeconds(Constants.ServerTimeToWait))
            {
                try
                {
                    lastReadTime = Send(firstClient, firstStream, secondStream,  lastReadTime);
                    lastReadTime = Send(secondClient,secondStream,firstStream, lastReadTime);
                }
                catch (Exception)
                {
                    break;
                }
            }

            firstClient.Close();
            secondClient.Close();
        }

        public DateTime Send(TcpClient client, NetworkStream firstStream , NetworkStream secondStream, DateTime lastReadTime)
        {
            var buffer = new byte[Constants.ChunkSize];
            while (client.Client.Poll(Constants.Timeout, SelectMode.SelectRead) && firstStream.DataAvailable)
            {
                    var bytes = firstStream.Read(buffer, 0, Constants.ChunkSize);
                    secondStream.Write(buffer, 0, bytes);
                    secondStream.Flush();
                    lastReadTime = DateTime.Now;
            }
            return lastReadTime;
        }
    }
}
