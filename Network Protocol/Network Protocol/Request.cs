using System;

namespace Network_Protocol
{
    public class Request
    {
    }

    public class CloseRequest: Request
    {
        public string ConnectionName { get { return "Test Connection"; } }
    }

    public class SomeRequest : Request
    {
        public int RandomNumber { get { return new Random().Next(); } }
    }
}
