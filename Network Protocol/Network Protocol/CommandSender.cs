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
        private TcpClient m_Client;
        private Thread m_HandleThread;
        private readonly BinaryFormatter m_BinaryFormatter = new BinaryFormatter();
        private  readonly JavaScriptSerializer m_JavaScriptSerializer = new JavaScriptSerializer();
        private readonly CancellationTokenSource m_Cts;
        private int m_Started = 0;
        private int m_Stopped = 0;

        public CommandSender()
        {
            m_CommandsQueue = new Queue<Command>();
            m_CommandsQueue.Enqueue(new SomeCommand(respone => Console.WriteLine("SomeCommand done")));
            m_CommandsQueue.Enqueue(new HelloWorldCommand(response => Console.WriteLine("HelloWorldCommandDone")));
            m_CommandsQueue.Enqueue(new SomeCommand((response)=>Console.WriteLine("Another SomeCommand Done")));
        }

        public CommandSender(TcpClient client, CommandFactory commandFactory,CancellationTokenSource cts)
            : this()
        {
            m_Cts = cts;
            m_Client = client;
            m_CommandFactory = commandFactory;
            m_Stream = client.GetStream();
        }

        public void AddCommand(Command command)
        {
            m_CommandsQueue.Enqueue(command);
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

        public void StopHandleCommands()
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
                if (m_CommandsQueue.Count != 0)
                {
                    foreach (var command in m_CommandsQueue)
                    {
                        command.SetCommandCompleted(new Response() { CommandResult = Result.Cancelled });
                    }
                }
                var closeCommand = new CloseCommand(response => m_Stream.Close());                
                ExecuteCommand(closeCommand);
                closeCommand.WaitHandle.WaitOne();
                m_Client.Close();
            }
        }

        private void Execute()
        {
            var token = m_Cts.Token;
            while (true)
            {
                if(token.IsCancellationRequested)
                    break;
                if (m_CommandsQueue.Count == 0)
                {
                    SpinWait.SpinUntil(() => m_CommandsQueue.Count != 0, 200);
                    continue;
                }
                ExecuteCommand(m_CommandsQueue.Dequeue());
            }
        }

        public void ExecuteCommand(Command command)
        {
            try
            {
                var commandID = m_CommandFactory.GetCommandID(command);

                var jsonRequest = m_JavaScriptSerializer.Serialize(command.Request);
                var jsonId = m_JavaScriptSerializer.Serialize(commandID);

                var jsonToSend = jsonId + ";" + jsonRequest;
                m_BinaryFormatter.Serialize(m_Stream,jsonToSend);

                var responseString = (string)m_BinaryFormatter.Deserialize(m_Stream);
                var response = m_JavaScriptSerializer.Deserialize(responseString, command.ResponseType);
                command.SetCommandCompleted((Response)response);
            }
            catch (IOException e)
            {
                command.Response.Message = e.Message;
                command.SetCommandCompleted(null);
            }
            catch (SocketException e)
            {
                command.Response.Message = e.Message;
                command.SetCommandCompleted(null);
            }
        }
    }
}
