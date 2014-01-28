using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Web.Script.Serialization;

namespace Network_Protocol
{
    public class CommandHandler
    {
        private readonly CommandFactory m_Factory;
        private Thread m_HandleThread;
        private readonly CommandHandlers m_Handlers = new CommandHandlers();
        private readonly Stream m_Stream;
        private readonly TcpClient m_Client;
        private readonly BinaryFormatter m_BinaryFormatter = new BinaryFormatter();
        private readonly JavaScriptSerializer m_JavaScriptSerializer = new JavaScriptSerializer();
        private int m_Started = 0;
        private int m_Stoped = 0;
        private CancellationTokenSource m_Cts ;

        public CommandHandler(TcpClient client, CommandFactory commandFactory,CancellationTokenSource cts)
        {
            m_Cts = cts;
            m_Client = client;
            m_Stream = client.GetStream();
            m_Factory = commandFactory;
        }

        public void StartHandleCommand()
        {
            if (Interlocked.Increment(ref m_Started) == 1)
            {
                m_HandleThread = new Thread(HandleCommands);
                m_HandleThread.Start();
            }
        }

        public void StopHandleCommands()
        {
            if (Interlocked.Increment(ref m_Stoped) == 1)
            {
                if (m_Started == 0)
                {
                    Interlocked.Decrement(ref m_Stoped);
                    return;
                }
                m_Cts.Cancel();
                m_Client.Close();
                m_HandleThread.Join();
                m_Stream.Close();
            }
        }

        public void HandleCommands()
        {
            var token = m_Cts.Token;
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }
                HandleCommand();
            }
        }

        private void HandleCommand()
        {
            Response response = null;
            try
            {
                var jsonCommandIDRequest = (string)m_BinaryFormatter.Deserialize(m_Stream);
                var jsonID = string.Empty;
                var jsonRequest = string.Empty;

                if (!string.IsNullOrEmpty(jsonCommandIDRequest))
                {
                    string[] parts = jsonCommandIDRequest.Split(';');
                    jsonID = parts[0];
                    jsonRequest = parts[1];
                }

                if (!string.IsNullOrEmpty(jsonID) && !string.IsNullOrEmpty(jsonRequest))
                {
                    var id = m_JavaScriptSerializer.Deserialize<int>(jsonID);
                    var command = m_Factory.GetCommandByID(id);
                    var request = m_JavaScriptSerializer.Deserialize(jsonRequest, command.RequestType);
                    command.Request = (Request)request;
                    response = ProcessCommand(command);
                    response.CommandResult = Result.Done;
                }
            }
            catch (Exception e)
            {
                response = new Response
                    {
                        CommandResult = Result.Failed,
                        Message = e.Message
                    };
            }

            string json = m_JavaScriptSerializer.Serialize(response);
            try
            {
                m_BinaryFormatter.Serialize(m_Stream, json);
            }
            catch (IOException e)
            {
            }
        }

        public Response ProcessCommand(Command command)
        {
            if (m_Handlers.ContainsCommandHandler(command.GetType()))
            {
                return m_Handlers[command.GetType()].Invoke(command);
            }
            return null;
        }
    }
}
