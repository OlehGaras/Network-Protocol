using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Web.Script.Serialization;

namespace Network_Protocol
{
    internal class CommandHandler
    {
        private readonly CommandFactory m_Factory;
        private Thread m_HandleThread;
        private readonly CommandHandlers m_Handlers = new CommandHandlers();
        private readonly Stream m_Stream;
        private readonly TcpClient m_Client;
        private readonly BinaryFormatter m_BinaryFormatter = new BinaryFormatter();
        private readonly JavaScriptSerializer m_JavaScriptSerializer = new JavaScriptSerializer();
        private int m_Started;
        private int m_Stoped;
        private readonly CancellationTokenSource m_Cts;
        private readonly object m_SyncObject = new object(); 

        public CommandHandler(TcpClient client, CommandFactory commandFactory, CancellationTokenSource cts)
        {
            m_Cts = cts;
            m_Client = client;
            m_Stream = client.GetStream();
            m_Factory = commandFactory;
            m_Handlers.AddHandler(typeof(CloseCommand), CloseCommandHandler);
        }

        private Response CloseCommandHandler(Command command)
        {
            m_Cts.Cancel();
            return new Response();         
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
                m_HandleThread.Join();
                m_Client.Close();          
                OnCloseConnection();
            }
        }

        public void AddHandler(Type command , Handler handler)
        {
            lock (m_SyncObject)
            {
                m_Handlers.AddHandler(command, handler);
            }
        }

        private void HandleCommands()
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
            if (Interlocked.Increment(ref m_Stoped) == 1)
            {
                m_Client.Close();
                OnCloseConnection();
            }
        }

        private void HandleCommand()
        {
            Response response = null;
            try
            {
                if (!m_Client.Client.Poll(200, SelectMode.SelectRead))
                    return;

                var jsonCommandIDRequest = (string) m_BinaryFormatter.Deserialize(m_Stream);
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
                    command.Request = (Request) request;
                    response = ProcessCommand(command);
                    response.CommandResult = Result.Done;
                }

                string json = m_JavaScriptSerializer.Serialize(response);
                m_BinaryFormatter.Serialize(m_Stream, json);
            }
            catch (IOException)
            {
                m_Cts.Cancel();
            }
            catch (SocketException)
            {
                m_Cts.Cancel();
            }
        }

        private Response ProcessCommand(Command command)
        {
            if (m_Handlers.ContainsCommandHandler(command.GetType()))
            {
                Handler handler;
                lock (m_SyncObject)
                {
                     handler = m_Handlers[command.GetType()];
                }
                Response response;
                try
                {
                    response = handler.Invoke(command);
                }
                catch (Exception e)
                {
                    response = command.Response;
                    response.CommandResult = Result.Failed;
                    response.Message = e.Message;
                }
                return response;
            }
            return command.Response;
        }

        public event EventHandler<EventArgs> CloseCommandHandled;
        protected virtual void OnCloseConnection()
        {
            EventHandler<EventArgs> handler = CloseCommandHandled;
            if (handler != null) 
                handler(this, EventArgs.Empty);
        }
    }
}
