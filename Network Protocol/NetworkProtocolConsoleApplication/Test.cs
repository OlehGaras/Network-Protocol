using System.Runtime.Serialization;

namespace NetworkProtocolConsoleApplication
{
    [DataContract]
    public class Test
    {
        [DataMember]
        public string Name { get; set; }

        public int Age { get; set; }
    }
}
