using System;
using System.IO;
using Network_Protocol;
using System.Runtime.Serialization;

namespace NetworkProtocolConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            //var server = new Server(7777);
            Test t = new Test();
            t.Name = "Text";
            t.Age = 100500;

            MemoryStream stream = new MemoryStream();
            DataContractSerializer ser = new DataContractSerializer(typeof(Test));
            ser.WriteObject(stream,t);
            stream.Position = 0;
            StreamReader sr = new StreamReader(stream);

            Console.WriteLine(sr.ReadToEnd());

            stream.Position = 0;

            Test t2 = (Test)ser.ReadObject(stream);
        }
    }
}
