using System;
using System.Collections.Generic;

namespace Network_Protocol
{
    public delegate Response Handler(Command command);
    public class CommandHandler
    {
        public Dictionary<Type, Handler> Handlers { get; set; }

        public CommandHandler()
        {
            Handlers = new Dictionary<Type, Handler>
                {
                    {typeof (CloseCommand), CloseCommandHandler},
                    {typeof (SomeCommand), SomeCommandHandler},
                    {typeof(HelloWorldCommand), HelloWorldCommandHandler}
                };
        }

        public void AddHandler(Command command, Handler handler)
        {
            Handlers.Add(command.GetType(), handler);
        }
        

        public bool ContainsCommandHandler(Type typeOfCommand)
        {
            return Handlers.ContainsKey(typeOfCommand);
        }

        public Handler this [Type typeOfCommand]
        {
            get { return Handlers[typeOfCommand]; }
        }

        public Response CloseCommandHandler(Command command)
        {
            return null;
        }

        public Response HelloWorldCommandHandler(Command command)
        {
            Console.WriteLine("Hello World Command Handled");
            return new Response();
        }

        public Response OpenCommandHandler()
        {           
            return null;
        }

        public Response SomeCommandHandler(Command command)
        {
            var r = new SomeResponse {Content = "Server processed the SomeCommand"};
            return r;
        }
    }
}
