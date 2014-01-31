namespace Network_Protocol
{
    public class Constants
    {
        public const string LineForHandshake = "ProtocolVersion:0.0.0.1";
        public const string ServerAnswer = "Accepted";
        public const string Request = "Send the request";
        public const string Separator = ";";
        public const int ServerTimeToWait = 500;
        public const int ChunkSize = 1024 * 4;
        public const int Timeout = 200;
        public const int PingTimeout = 300;
    }
}
