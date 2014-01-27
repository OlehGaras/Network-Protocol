namespace Network_Protocol
{
    public class Response
    {
        public Result CommandResult { get; set;}
        public string Message { get; set;}
    }

    public class SomeResponse : Response
    {
        public string Content = "SomeResponse";
    }
}
