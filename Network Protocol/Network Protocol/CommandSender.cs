using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace Network_Protocol
{
    public class CommandSender
    {
        private Stream m_Stream;
        public Queue<Command> CommandsQueue { get; set; }
        private DisplayCommandFactory m_CommandFactory;
        private StreamReader m_Reader;
        private StreamWriter m_Writer;

        public CommandSender()
        {
            m_CommandFactory = new DisplayCommandFactory();
            CommandsQueue = new Queue<Command>();
            CommandsQueue.Enqueue(new SomeCommand());

            CommandsQueue.Enqueue(new CloseCommand());
        }

        public CommandSender(Stream stream)
            : this()
        {
            m_Stream = stream;
            m_Reader = new StreamReader(stream);
            m_Writer = new StreamWriter(stream)
                {
                    AutoFlush = true
                };
        }

        public void AddCommand(Command c)
        {
            m_CommandFactory.AddCommand(c.GetType());
            CommandsQueue.Enqueue(c);
        }

        public void Execute()
        {
            while (CommandsQueue.Count != 0)
            {
                ExecuteCommand(CommandsQueue.Peek());
                CommandsQueue.Dequeue();
            }
            m_Stream.Close();
            m_Reader.Close();
            m_Writer.Close();
        }

        public void ExecuteCommand(Command command)
        {
            var id = m_CommandFactory.GetCommandID(command);
            var jsonId = new JavaScriptSerializer().Serialize(id);
            m_Writer.WriteLine(jsonId);
            if(String.CompareOrdinal(m_Reader.ReadLine(), Constants.Request) == 0)
            {
                var jsonRequest = new JavaScriptSerializer().Serialize(command.Request);
                m_Writer.WriteLine(jsonRequest);
            }
            var jsonResponse = m_Reader.ReadLine();
            if (jsonResponse != null)
            {
                var response = new JavaScriptSerializer().Deserialize(jsonResponse,command.Response.GetType());
                command.CallBackMethod.Invoke((Response)response);
            }
        }
    }
}
