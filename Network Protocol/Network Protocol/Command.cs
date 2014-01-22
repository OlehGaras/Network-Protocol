using System;
using System.Runtime.Serialization;

namespace Network_Protocol
{
    [DataContract]
    public  class Command
    {
        protected virtual Type RequestType { get { return typeof (Request); } }
        protected virtual Type ResponseType { get { return typeof (Response); } }

        private Request m_Request;

        [DataMember]
        public Request Request
        {
            get
            {
                if (m_Request == null)
                {
                    m_Request = (Request)Activator.CreateInstance(RequestType);
                }
                return m_Request;
            }

            private set
            {
                m_Request = value;
            }
        }

        private Response m_Response;
        public Response Response {
            get
            {
                if (m_Response == null)
                {
                    m_Response = (Response) Activator.CreateInstance(ResponseType);
                }
                return m_Response;
            }
            private set
            {
                m_Response = value;
            }
        }

        [DataMember]
        public int Id { get; private set; }

        public delegate void CallBack();
    }

    public class CloseCommand : Command
    {
        protected override Type RequestType
        {
            get { return typeof (CloseRequest); }
        }
    }
}
