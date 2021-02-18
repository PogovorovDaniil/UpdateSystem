using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client_US
{
    class Program
    {
        static Socket socket;
        static EndPoint endPoint;
        static string IP = "127.0.0.1";
        static int port = 9090;

        static FileAndHash[] GetFileListFromServer()
        {

            Console.WriteLine("Start");
            socket.Send(new byte[] {0});

            byte[] sizeReceive = new byte[4];
            socket.Receive(sizeReceive, 0, 4, SocketFlags.None);
            uint size = 0;
            for(int i = 0; i < 4; i++)
            {
                size *= 0x100;
                size += sizeReceive[i];
            }
            Console.WriteLine("Size: " + size);

            FileStream fs = File.OpenWrite("serverList.txt");
            fs.SetLength(0);

            Console.WriteLine("Download");

            byte[] buffer = new byte[4096];
            int offset = 0;
            int bytesCount;
            while(offset < size)
            {
                bytesCount = socket.Receive(buffer);
                fs.Write(buffer, 0, bytesCount);
                offset += bytesCount;
                Console.WriteLine("Downloaded: " + offset);
            }
            fs.Close();
            Console.WriteLine("Download end\n");


            List<FileAndHash> fileAndHashes = new List<FileAndHash>();

            StreamReader sr = new StreamReader("serverList.txt");
            while (!sr.EndOfStream)
            {
                FileAndHash fileAndHash = FileAndHash.FromString(sr.ReadLine());
                if(fileAndHash.fileHash != "") fileAndHashes.Add(fileAndHash);
            }
            sr.Close();
            return fileAndHashes.ToArray();
        }

        private static void DeleteDirs(string path)
        {
            foreach (string directory in Directory.GetDirectories(path))
            {
                DeleteDirs(directory);
                if (Directory.GetFiles(directory).Length + Directory.GetDirectories(directory).Length == 0)
                {
                    Directory.Delete(directory);
                }
            }
        }

        static void DownloadFile(string fileName)
        {
            Console.WriteLine("Start");
            byte[] bfileName = Encoding.UTF8.GetBytes(fileName);
            socket.Send(new byte[] { 1, (byte)bfileName.Length });
            socket.Send(bfileName);

            byte[] sizeReceive = new byte[4];
            socket.Receive(sizeReceive, 0, 4, SocketFlags.None);
            uint size = 0;
            for (int i = 0; i < 4; i++)
            {
                size *= 0x100;
                size += sizeReceive[i];
            }
            Console.WriteLine("Size: " + size);

            string fileDir = Directory.GetCurrentDirectory() + "/dir/" + fileName;
            Directory.CreateDirectory(fileDir);
            Directory.Delete(fileDir, true);

            FileStream fs = File.OpenWrite(fileDir);
            fs.SetLength(0);

            Console.WriteLine("Download");

            byte[] buffer = new byte[4096];
            int offset = 0;
            int bytesCount;
            while (offset < size)
            {
                bytesCount = socket.Receive(buffer);
                fs.Write(buffer, 0, bytesCount);
                offset += bytesCount;
                Console.WriteLine("Downloaded: " + offset);
            }
            fs.Close();
            Console.WriteLine("Download end\n");
        }

        static void DeleteFile(string fileName)
        {
            string path = Directory.GetCurrentDirectory() + "/dir/" + fileName;
            if (Directory.Exists(path))
                Directory.Delete(path, true);
            File.Delete(path);
        }

        static void Main()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            endPoint = new IPEndPoint(IPAddress.Parse(IP), port);

            socket.Connect(endPoint);
            Console.WriteLine("Connected");
            try
            {
                FileAndHash[] serverFileList = GetFileListFromServer();
                FileAndHash[] clientFileList = HashList.GetFileList(Directory.GetCurrentDirectory() + "\\dir\\");

                FileManage[] fileManages = FileManager.Manage(serverFileList, clientFileList);
                foreach (FileManage file in fileManages) Console.WriteLine(file);
                Console.ReadKey(true);

                foreach (FileManage file in fileManages)
                {
                    if (file.fileAction == FileAction.Delete || file.fileAction == FileAction.Replace)
                    {
                        DeleteFile(file.fileName);
                    }
                    if (file.fileAction == FileAction.Create || file.fileAction == FileAction.Replace)
                    {
                        DownloadFile(file.fileName);
                    }
                }

                DeleteDirs(Directory.GetCurrentDirectory() + "/dir/");
            }
            catch(Exception ex)
            {
                socket.Close();
                Console.WriteLine("Lost connection");
                Console.WriteLine(ex);
            }
            Console.WriteLine("End receive");
            while (true) ;
        }
    }
}
