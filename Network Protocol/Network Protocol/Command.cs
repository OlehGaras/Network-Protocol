using System;
using System.Threading;

namespace Network_Protocol
{
    public delegate void CallBack(Response response);

    public class Command
    {
        public virtual Type RequestType { get { return typeof(Request); } }
        public virtual Type ResponseType { get { return typeof(Response); } }
        protected CallBack CallBackMethod { get; set; }

        protected Command(CallBack callBack = null)
        {
            CallBackMethod = callBack;
        }

        private Request m_Request;
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

            set
            {
                if (value.GetType() != RequestType)
                {
                    throw new Exception("Bad request Type");
                }

                m_Request = value;
            }
        }

        private Response m_Response;
        public Response Response
        {
            get
            {
                if (m_Response == null)
                {
                    m_Response = (Response)Activator.CreateInstance(ResponseType);
                }
                return m_Response;
            }
            set
            {
                if (value.GetType() != ResponseType)
                {
                    throw new Exception("Bad response Type");
                }

                m_Response = value;
            }
        }

        private ManualResetEvent m_WaitHandle;
        public WaitHandle WaitHandle
        {
            get
            {
                if (m_WaitHandle == null)
                {
                    m_WaitHandle = new ManualResetEvent(false);
                }
                return m_WaitHandle;
            }
        }

        public void SetCommandCompleted(Response response)
        {
            Response = response;
            if (CallBackMethod != null)
                CallBackMethod(response);
            if (m_WaitHandle == null)
            {
                m_WaitHandle = new ManualResetEvent(false);
            }
            m_WaitHandle.Set();
        }
    }

    public class SomeCommand : Command
    {
        public SomeCommand()
            : this(null)
        {
        }

        public SomeCommand(CallBack callBack = null)
            : base(callBack)
        { }

        public override Type RequestType
        {
            get
            {
                return typeof(SomeRequest);
            }
        }
    }

    public class HelloWorldCommand : Command
    {

        public HelloWorldCommand()
            : this(null)
        {
        }

        public HelloWorldCommand(CallBack callback = null)
            : base(callback)
        {
        }
    }
}
