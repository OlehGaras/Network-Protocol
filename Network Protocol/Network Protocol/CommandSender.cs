using System.Collections.Generic;
using System.IO;

namespace Network_Protocol
{
    public delegate void CallBack();
    public class CommandSender
    {
        public Stream Stream;
        public Queue<Command, CallBack> CommandsQueue { get; set; }

        public CommandSender()
        {
            CommandsQueue = new Queue<Command, CallBack>();
        }

        public CommandSender(Stream stream)
            : this()
        {
            Stream = stream;
        }

        public void AddCommand(Command c, CallBack method)
        {
            CommandsQueue.Add(c, method);
        }

        private DisplayCommandFactory CommandFactory;

        public void ExecuteCommand(Command command)
        {
            var streamWriter = new StreamWriter((Stream) null);
            var id = CommandFactory.GetCommandID(command);
            streamWriter.Write(id);
            streamWriter.Write(Request);
        }

        public string Send(StreamWriter streamWriter, StreamReader streamReader)
        {
            var commands = new Command[CommandsQueue.Count];
            var callbacks = new CallBack[CommandsQueue.Count];
            CommandsQueue.Keys.CopyTo(commands, 0);
            CommandsQueue.Values.CopyTo(callbacks, 0);

            string json = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(commands[commands.Length - 1]);
            streamWriter.WriteLine(json);

            var jsonResponse = streamReader.ReadLine();
            var typeOfResponse = commands[commands.Length - 1].Response.GetType();
            var response = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize <typeOfResponse> (jsonResponse);
            if (response != null)
            {
                callbacks[callbacks.Length-1].Invoke();
            }

            return json;
        }

    }
}
