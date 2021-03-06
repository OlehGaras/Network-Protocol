﻿using System;
using System.Collections.Generic;

namespace Network_Protocol
{
    public delegate Response Handler(Command command);
    internal class CommandHandlers
    {
        public Dictionary<Type, Handler> Handlers { get; set; }

        public CommandHandlers()
        {
            Handlers = new Dictionary<Type, Handler>
                {
                    {typeof (SomeCommand), SomeCommandHandler},
                    {typeof(PingCommand), PingCommandHandler}
                };
        }

        public void AddHandler(Type command, Handler handler)
        {
            Handlers.Add(command, handler);
        }


        public bool ContainsCommandHandler(Type typeOfCommand)
        {
            return Handlers.ContainsKey(typeOfCommand);
        }

        public Handler this[Type typeOfCommand]
        {
            get { return Handlers[typeOfCommand]; }
        }

        private static Response PingCommandHandler(Command command)
        {
            return new Response();
        }

        private Response SomeCommandHandler(Command command)
        {
            var r = new SomeResponse { Content = "Server processed the SomeCommand" };
            return r;
        }
    }
}
