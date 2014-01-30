using System;

namespace Network_Protocol
{
    internal class CloseCommand : Command
    {
        public CloseCommand():this(null)
        {
        }
        public CloseCommand(CallBack callBack) : base(callBack)
        {
        }

        public override Type RequestType
        {
            get { return typeof(CloseRequest); }
        }
        public override Type ResponseType
        {
            get { return typeof (CloseResponse); }
        }
    }
}