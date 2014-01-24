using System;
using System.IO;
using System.Text;
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
        private bool m_IsEnd;

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
                    var data = new byte[1000];
                    m_Stream.BeginRead(data, 0, data.Length, GotCommand, data);
                    if (m_IsEnd)
                        break;
                }
                //m_Reader.Close();
                //m_Writer.Close();
                //m_Stream.Close();
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

        private void GotCommand(IAsyncResult ar)
        {
            try
            {
                var readBytes = m_Stream.EndRead(ar);
                var data = (byte[]) ar.AsyncState;
                var jsonCommandIDRequest = Encoding.UTF8.GetString(data, 0, readBytes);
                var jsonID = string.Empty;
                var jsonRequest = string.Empty;

                if (jsonCommandIDRequest != "")
                {
                    string[] parts = jsonCommandIDRequest.Split(';');
                    jsonID = parts[0];
                    jsonRequest = parts[1];
                }

                if (jsonID != "" && jsonRequest != "")
                {
                    var id = new JavaScriptSerializer().Deserialize<int>(jsonID);
                    var command = m_Factory.GetCommandByID(id);

                    if (command.GetType() == typeof (CloseCommand))
                        m_IsEnd = true;

                    var t = command.Request.GetType();
                    var request = new JavaScriptSerializer().Deserialize(jsonRequest, t);

                    command.SetRequest((Request) request);

                    var response = ProcessCommand(command);
                    string json = new JavaScriptSerializer().Serialize(response);

                    byte[] arrResponse = Encoding.UTF8.GetBytes(json);
                    m_Stream.BeginWrite(arrResponse, 0, arrResponse.Length, WriteDone, null);
                }
            }
            catch (IOException)
            {
            }

        }

        private void WriteDone(IAsyncResult ar)
        {
            try
            {
                m_Stream.EndWrite(ar);
                m_IsEnd = true;
                m_Stream.Close();
            }
            catch (IOException)
            {
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
