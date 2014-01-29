using System;

namespace Network_Protocol
{
    public class EndpointEventArgs : EventArgs
    {
        public EndPoint EndPoint { get; private set; }
        public EndpointEventArgs(EndPoint endPoint)
        {
            if (endPoint == null) 
                throw new ArgumentNullException("endPoint");
            EndPoint = endPoint;
        }
    }
}