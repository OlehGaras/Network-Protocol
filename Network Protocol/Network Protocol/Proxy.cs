using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace Network_Protocol
{
    public class Proxy
    {
        private const int ChunkSize = 4096;
        private readonly TcpClient m_FirstInClient;
        private readonly TcpClient m_FirstOutClient;
        private readonly TcpClient m_SecondInClient;
        private readonly TcpClient m_SecondOutClient;

        public Proxy(EndPointExtention first, EndPointExtention second):this(first.InClient,first.OutClient,second.InClient,second.OutClient)
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
            var firstInStream = m_FirstInClient.GetStream();
            var secondInStream = m_SecondInClient.GetStream();
            var firstOutStream = m_FirstOutClient.GetStream();
            var secondOutStream = m_SecondOutClient.GetStream();

            var sendToSecondThread = new Thread(() => FromFirstToSecond(firstInStream, secondOutStream));
            sendToSecondThread.Start();

            var sendToFirstThread = new Thread(() => FromSecondToFirst(firstOutStream, secondInStream));
            sendToFirstThread.Start();
        }

        public void FromFirstToSecond(Stream firstInStream,Stream secondOutStream)
        {
            var buffer = new byte[ChunkSize];
          

            var res1 = firstInStream.Read(buffer, 0, buffer.Length);
            while (res1 != 0)
            {
                secondOutStream.Write(buffer,0,buffer.Length);
                res1 = firstInStream.Read(buffer, 0, buffer.Length);
            }

            firstInStream.Close();
            secondOutStream.Close();

        }

        public void FromSecondToFirst(Stream firstOutStream, Stream secondInStream)
        {
            var buffer = new byte[ChunkSize];


            var res1 = firstOutStream.Read(buffer, 0, buffer.Length);
            while (res1 != 0)
            {
                secondInStream.Write(buffer, 0, buffer.Length);
                res1 = firstOutStream.Read(buffer, 0, buffer.Length);
            }

            firstOutStream.Close();
            secondInStream.Close();

        }

    }
}
