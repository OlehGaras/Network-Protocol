using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace Network_Protocol
{
    public class Proxy
    {
        private const int ChunkSize = 4;
        private readonly TcpClient m_FirstInClient;
        private readonly TcpClient m_FirstOutClient;
        private readonly TcpClient m_SecondInClient;
        private readonly TcpClient m_SecondOutClient;
        private readonly Dictionary<Guid, Guid> m_SharedEndPoints = new Dictionary<Guid, Guid>();  

        public Proxy(EndPointExtention first, EndPointExtention second)
            : this(first.InClient, first.OutClient, second.InClient, second.OutClient)
        {
            m_SharedEndPoints.Add(first.Guid,second.Guid);
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

            var sendToSecondThread = new Thread(() => SendFromTo(firstInStream, firstOutStream,secondInStream,secondOutStream));
            sendToSecondThread.Start();
            sendToSecondThread.Join();

            var sendToFirstThread = new Thread(() => SendFromTo(secondInStream,secondOutStream,firstInStream,firstOutStream));
            sendToFirstThread.Start();
        }

        public void SendFromTo(Stream firstInStream, Stream firstOutStream, Stream secondInStream, Stream secondOutStream)
        {
            var buffer1 = new byte[ChunkSize];
            var buffer2 = new byte[ChunkSize];

            var res1 = firstInStream.Read(buffer1, 0, buffer1.Length);
            var res2 = secondOutStream.Read(buffer2, 0, buffer2.Length);
            firstInStream.Position -= 4;
            secondOutStream.Position -= 4;
            while (res1 != 0 && res2 !=0)
            {
                secondInStream.Write(buffer1, 0, buffer1.Length);
                firstOutStream.Write(buffer2, 0, buffer2.Length);

                firstInStream.Position -= 4;
                secondOutStream.Position -= 4;
                res1 = firstInStream.Read(buffer1, 0, buffer1.Length);
                res2 = firstOutStream.Read(buffer2, 0, buffer2.Length);

            }

            firstInStream.Close();
            firstOutStream.Close();
            secondInStream.Close();
            secondOutStream.Close();
        }

    }
}
