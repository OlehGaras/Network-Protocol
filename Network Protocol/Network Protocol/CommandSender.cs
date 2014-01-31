using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Web.Script.Serialization;

namespace Network_Protocol
{
    public class CommandSender
    {
        private readonly Stream m_Stream;
        private readonly Queue<Command> m_CommandsQueue;
        private readonly CommandFactory m_CommandFactory;
        private readonly TcpClient m_Client;
        private readonly BinaryFormatter m_BinaryFormatter = new BinaryFormatter();
        private readonly JavaScriptSerializer m_JavaScriptSerializer = new JavaScriptSerializer();
        private readonly CancellationTokenSource m_Cts;
        private Thread m_HandleThread;
        private int m_Started;
        private int m_Stopped;
        private static readonly object m_SyncObject = new object();

        public CommandSender()
        {
            m_CommandsQueue = new Queue<Command>();
        }

        public CommandSender(TcpClient client, CommandFactory commandFactory, CancellationTokenSource cts)
            : this()
        {
            m_Cts = cts;
            m_Client = client;
            m_CommandFactory = commandFactory;
            m_Stream = client.GetStream();
        }

        public void AddCommand(Command command)
        {
            lock (m_SyncObject)
            {
                m_CommandsQueue.Enqueue(command);
            }
        }

        public void StartHandleCommands()
        {
            if (Interlocked.Increment(ref m_Started) == 1)
            {
                m_HandleThread = new Thread(Execute);
                m_HandleThread.Start();
            }
            else
            {
                throw new Exception("Cannot start this method twice");
            }
        }

        public void StopHandleCommands(bool executeCloseCommand = true)
        {
            if (Interlocked.Increment(ref m_Stopped) == 1)
            {
                if (m_Started == 0)
                {
                    Interlocked.Decrement(ref m_Stopped);
                    return;
                }
                m_Cts.Cancel();
                m_HandleThread.Join();

                CheckTheQueue();

                if (executeCloseCommand)
                {
                    var closeCommand = new CloseCommand(response => m_Stream.Close());
                    ExecuteCommand(closeCommand);
                    closeCommand.WaitHandle.WaitOne();
                }
                m_Client.Close();
                OnConnectionLost();
            }
        }

        private void CheckTheQueue()
        {
            lock (m_SyncObject)
            {
                if (m_CommandsQueue.Count != 0)
                {
                    foreach (var command in m_CommandsQueue)
                    {
                        command.Response.CommandResult = Result.Cancelled;
                        command.SetCommandCompleted(command.Response);
                    }
                }   
            }
        }

        private void Execute()
        {
            var token = m_Cts.Token;
            DateTime lastCommandTime = DateTime.Now;
            while (true)
            {
                if (token.IsCancellationRequested)
                    break;
                if (m_CommandsQueue.Count == 0 && (DateTime.Now - lastCommandTime) < TimeSpan.FromMilliseconds(Constants.PingTimeout))
                {
                    SpinWait.SpinUntil(() => m_CommandsQueue.Count != 0, Constants.Timeout);
                    continue;
                }
                Command command;
                lock (m_SyncObject)
                {
                    command = m_CommandsQueue.Count == 0 ? new PingCommand() : m_CommandsQueue.Dequeue();
                }
                ExecuteCommand(command);
                lastCommandTime = DateTime.Now;
            }
            if (Interlocked.Increment(ref m_Stopped) == 1)
            {
                m_Client.Close();
                CheckTheQueue();
                OnConnectionLost();
            }
        }

        private void ExecuteCommand(Command command)
        {
            try
            {
                var commandID = m_CommandFactory.GetCommandID(command);

                var jsonRequest = m_JavaScriptSerializer.Serialize(command.Request);
                var jsonId = m_JavaScriptSerializer.Serialize(commandID);

                var jsonToSend = jsonId + Constants.Separator + jsonRequest;
                m_BinaryFormatter.Serialize(m_Stream, jsonToSend);

                var responseString = (string)m_BinaryFormatter.Deserialize(m_Stream);
                var response = m_JavaScriptSerializer.Deserialize(responseString, command.ResponseType);
                command.SetCommandCompleted((Response)response);
            }
            catch (IOException e)
            {
                command.Response.Message = e.Message;
                command.Response.CommandResult = Result.Cancelled;
                command.SetCommandCompleted(command.Response);
                m_Cts.Cancel();

            }
            catch (SocketException e)
            {
                command.Response.Message = e.Message;
                command.Response.CommandResult = Result.Cancelled;
                command.SetCommandCompleted(command.Response);
                m_Cts.Cancel();
            }
        }

        public event EventHandler<EventArgs> ConnectionLost;
        protected virtual void OnConnectionLost()
        {
            var handler = ConnectionLost;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}
