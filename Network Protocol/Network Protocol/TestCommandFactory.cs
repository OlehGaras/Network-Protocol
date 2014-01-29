namespace Network_Protocol
{
    public class TestCommandFactory : CommandFactory
    {
        public TestCommandFactory()
        {            
            AddCommand(typeof(SomeCommand));
            AddCommand(typeof(CloseCommand));
            AddCommand(typeof(HelloWorldCommand));
        }
    }
}