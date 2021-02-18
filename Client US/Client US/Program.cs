using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Client_US
{
    class Program
    {
        static void Main()
        {
            //FileAndHash[] files = HashList.GetFileList(Directory.GetCurrentDirectory() + "\\dir\\");
            //foreach (FileAndHash file in files) Console.WriteLine(file);

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            EndPoint endPoint = new IPEndPoint(IPAddress.Parse("193.178.169.223"), 9090);

            socket.Connect(endPoint);
            while (true)
            {
                int a = int.Parse(Console.ReadLine());
                int b = int.Parse(Console.ReadLine());
                socket.Send(new byte[] {(byte)a, (byte)b});
                byte[] buffer = new byte[1];
                socket.Receive(buffer);

                Console.WriteLine(buffer[0]);
            }
        }
    }
}
