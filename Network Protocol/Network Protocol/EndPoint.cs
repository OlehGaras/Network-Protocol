using System;
using System.IO;
using System.Web.Script.Serialization;

namespace Network_Protocol
{
    public class EndPoint
    {
        private DisplayCommandFactory m_Factory = new DisplayCommandFactory();
        private readonly CommandHandler m_Handler = new CommandHandler();
        private Stream m_Stream;
        private StreamReader m_Reader;
        private StreamWriter m_Writer;

        public EndPoint(Stream stream)
        {
            m_Stream = stream;
            m_Reader = new StreamReader(stream);
            m_Writer = new StreamWriter(stream)
                {
                    AutoFlush = true
                };
        }

        public void HandleCommands()
        {
            try
            {
                while (m_Stream.CanRead)
                {
                    string jsonCommandID = m_Reader.ReadLine();
                    m_Writer.WriteLine(Constants.Request);
                    string jsonCommandRequest = m_Reader.ReadLine();

                    if (jsonCommandID != null && jsonCommandRequest != null)
                    {
                        var id = new JavaScriptSerializer().Deserialize<int>(jsonCommandID);
                        var command = m_Factory.GetCommandByID(id);

                        if (command.GetType() == typeof (CloseCommand))
                            break;

                        Type t = command.Request.GetType();
                        var request = new JavaScriptSerializer().Deserialize(jsonCommandRequest,t);

                        command.SetRequest((Request)request);

                        var response = ProcessCommand(command);
                        string json = new JavaScriptSerializer().Serialize(response);
                        m_Writer.WriteLine(json);
                    }
                }
                m_Reader.Close();
                m_Writer.Close();
                m_Stream.Close();
            }
            catch (IOException)
            {
                //HERE GOES CODE TO HANDLE CLIENT DISCONNECTION
            }
            catch (ObjectDisposedException)
            {
                //HERE GOES CODE TO HANDLE CLIENT DISCONNECTION
            }
        }

        public Response ProcessCommand(Command command)
        {
            if (m_Handler.ContainsCommandHandler(command.GetType()))
            {                
                return m_Handler[command.GetType()].Invoke(command);
            }
            return null;
        }
    }
}
