using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace Network_Protocol
{
    public class CommandSender
    {
        private readonly Stream m_Stream;
        public Queue<Command> CommandsQueue { get; set; }
        private readonly DisplayCommandFactory m_CommandFactory;
        private readonly StreamReader m_Reader;
        private readonly StreamWriter m_Writer;
        private Command m_CurrentCommand;

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
            //m_Stream.Close();
            //m_Reader.Close();
            //m_Writer.Close();
        }

        public void ExecuteCommand(Command command)
        {
            m_CurrentCommand = command;
            var id = m_CommandFactory.GetCommandID(command);
            var jsonRequest = new JavaScriptSerializer().Serialize(command.Request);
            var jsonId = new JavaScriptSerializer().Serialize(id);
            var jsonToSend = jsonId + ";" + jsonRequest;
            var data = Encoding.UTF8.GetBytes(jsonToSend);

            m_Stream.BeginWrite(data, 0, data.Length, WriteDone, null);
            //m_Writer.WriteLine(jsonToSend);
            //var jsonResponse = m_Reader.ReadLine();
            //if (jsonResponse != null)
            //{
            //    var response = new JavaScriptSerializer().Deserialize(jsonResponse,command.Response.GetType());
            //    command.CallBackMethod.Invoke((Response)response);
            //}
        }

        private void WriteDone(IAsyncResult ar)
        {
            m_Stream.EndWrite(ar);

            byte[]data = new byte[1000];
            m_Stream.BeginRead(data, 0, data.Length, GotResponse, data);
        }

        private void GotResponse(IAsyncResult ar)
        {
            int readBytes = m_Stream.EndRead(ar);

            var arrResponse = (byte[]) ar.AsyncState;
            var jsonResponse = Encoding.UTF8.GetString(arrResponse, 0, readBytes);
            var response = new JavaScriptSerializer().Deserialize(jsonResponse, m_CurrentCommand.Response.GetType());
            m_CurrentCommand.CallBackMethod.Invoke((Response)response);
            m_Stream.Close();
        }
    }
}
