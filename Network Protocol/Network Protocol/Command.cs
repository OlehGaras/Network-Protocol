using System;
using System.Runtime.Serialization;

namespace Network_Protocol
{
    public delegate void CallBack(Response response);
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
        public int Id { get; set; }

        public void SetRequest(Request request)
        {
            Request = request;
        }

        public CallBack CallBackMethod { get; set; }
    }

    public class CloseCommand : Command
    {
        public CloseCommand()
        {
            Id = "CloseCommand".GetHashCode();
            CallBackMethod = new CallBack((response) =>
                {
                    
                });
        }
        protected override Type RequestType
        {
            get { return typeof (CloseRequest); }
        }
    }

    public class SomeCommand : Command
    {
        public SomeCommand()
        {
            Id = "SomeCommand".GetHashCode();
            CallBackMethod = new CallBack((response) =>
            {

            });
           
        }
        protected override Type RequestType
        {
            get
            {
                return typeof(SomeRequest);
            }
        }
    }

    public class HelloWorldCommand : Command
    {
    }
}
