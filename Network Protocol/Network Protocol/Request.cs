using System;

namespace Network_Protocol
{
    public class Request
    {
    }

    public class CloseRequest: Request
    {
        public int ConnectionName { get { return new Random().Next(); } }
    }
}
