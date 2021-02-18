using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Client_US
{
    class Program
    {
        static Socket socket;
        static EndPoint endPoint;

        static void Main()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            endPoint = new IPEndPoint(IPAddress.Parse("193.178.169.223"), 9090);

            socket.Connect(endPoint);
            Console.WriteLine("Connected");
            try
            {
                while (true)
                {
                    //FileAndHash[] files = HashList.GetFileList(Directory.GetCurrentDirectory() + "\\dir\\");
                    //foreach (FileAndHash file in files) Console.WriteLine(file);

                    int act = int.Parse(Console.ReadLine());
                    socket.Send(new byte[] {(byte)act});
                    byte[] buffer = new byte[1];
                    int bytesCount = socket.Receive(buffer);
                    if (bytesCount == 0) break;

                    Console.WriteLine(buffer[0]);
                }
            }
            catch(Exception)
            {
                socket.Close();
                Console.WriteLine("Lost connection");
            }
            Console.WriteLine("End receive");
            while (true) ;
        }
    }
}
