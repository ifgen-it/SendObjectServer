using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Xml.Serialization;

namespace SendObjectServer
{
    class Program
    {
        static int port = 8005;
        static string localHost = "127.0.0.1";

        static void Main(string[] args)
        {

            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(localHost), port);
            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                listenSocket.Bind(ipPoint);
                listenSocket.Listen(10);
                Console.WriteLine("Server was started. Waiting for connections...");

                while (true)
                {
                    Socket handler = listenSocket.Accept();

                    // GET THE MESSAGE
                    int bytes = 0;
                    byte[] data = new byte[2256];
                    List<byte> reciever = new List<byte>();
                    do
                    {
                        bytes = handler.Receive(data);
                        byte[] buffer = null;
                        if (bytes > 0)
                        {
                            buffer = new byte[bytes];
                            for (int i = 0; i < bytes; i++)
                            {
                                buffer[i] = data[i];
                            }
                        }
                        reciever.AddRange(buffer);
                    } while (handler.Available > 0);

                    byte[] answer = reciever.ToArray();

                    PersonMessage p1 = XMLByteArrayToPersonMessage(answer);
                    Console.WriteLine("\nDeserialized person message:\n\n" + p1.ToString());
                    byte[] foto = p1.data;
                    File.WriteAllBytes("newPic.jpg", foto);

                    //Console.WriteLine(DateTime.Now.ToShortDateString() + ": " + builder.ToString());

                    // SEND THE ANSWER
                    string message = "your message delivered";
                    data = Encoding.Unicode.GetBytes(message);
                    handler.Send(data);

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }

        }


        public static byte[] ObjectToByteArray(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static Object ByteArrayToObject(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = binForm.Deserialize(memStream);
                return obj;
            }
        }

        public static byte[] PersonMessageToXMLByteArray(PersonMessage obj)
        {
            XmlSerializer xmlSer = new XmlSerializer(typeof(PersonMessage));
            using (var ms = new MemoryStream())
            {
                xmlSer.Serialize(ms, obj);
                return ms.ToArray();
            }

        }

        public static PersonMessage XMLByteArrayToPersonMessage(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                XmlSerializer xmlSer = new XmlSerializer(typeof(PersonMessage));

                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                PersonMessage person = (PersonMessage)xmlSer.Deserialize(memStream);

                return person;
            }
        }


    }
}
