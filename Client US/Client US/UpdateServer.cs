using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Client_US
{
    class UpdateServer
    {
        public delegate void ProgressLoadUpdateHandler(int percent, int progress, int size);
        public event ProgressLoadUpdateHandler IsProgressLoadUpdate;

        private Socket socket;
        private EndPoint endPoint;

        private bool isConnect;
        private int lastPercent;

        private int progressDownload;
        private int sizeToDownload;

        private FileManage[] fileManages;
        string[] ignoreList;
        string lastVersion;
        string version;

        public UpdateServer(string ip, int port)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            endPoint = new IPEndPoint(IPAddress.Parse(ip), port);

            isConnect = false;
        }
        public bool TryConnect(out int size)
        {
            try
            {
                socket.Connect(endPoint);
                isConnect = true;

                GetlastVersion();
                ignoreList = GetIgnoreList();
                FileAndHash[] serverFileList = GetFileListFromServer();

                /*
                foreach (string ignore in ignoreList) Console.WriteLine(ignore);
                Console.WriteLine();
                Console.WriteLine();

                foreach (FileAndHash file in serverFileList) Console.WriteLine(file);
                Console.WriteLine();
                Console.WriteLine();

                Console.WriteLine();
                */
                sizeToDownload = 0;
                if (Compare(serverFileList, GetFileList(Directory.GetCurrentDirectory(), false), ignoreList) || version != lastVersion)
                {
                    FileAndHash[] clientFileList = GetFileList(Directory.GetCurrentDirectory());
                    fileManages = Manage(serverFileList, clientFileList, ignoreList);

                    /*
                    foreach (FileAndHash file in clientFileList) Console.WriteLine(file);
                    Console.WriteLine();
                    Console.WriteLine();

                    foreach (FileManage file in fileManages) Console.WriteLine(file);
                    Console.WriteLine();
                    Console.WriteLine();
                    */

                    foreach (FileManage fileManage in fileManages)
                    {
                        if (fileManage.fileSize > 0) sizeToDownload += fileManage.fileSize;
                    }
                }
            }
            catch
            {
                isConnect = false;
                size = 0;
                return false;
            }
            size = sizeToDownload;
            return true;
        }
        public bool UpdateFileSystem()
        {
            if (isConnect)
            {
                try
                {
                    if(sizeToDownload > 0)
                    {
                        lastPercent = 0;
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

                        DeleteDirs(Directory.GetCurrentDirectory() + "/");
                    }
                }
                catch
                {
                    socket.Close();
                    isConnect = false;
                    return false;
                }
                return true;
            }
            return false;
        }

        public enum FileAction { Create, Replace, Delete };
        public struct FileManage
        {
            public string fileName;
            public FileAction fileAction;
            public int fileSize;

            public FileManage(string fileName, FileAction fileAction, int fileSize)
            {
                this.fileName = fileName;
                this.fileAction = fileAction;
                this.fileSize = fileSize;
            }

            public override string ToString()
            {
                return fileName + " " + fileAction;
            }
        }
        public struct FileAndHash
        {
            public string fileName;
            public string fileHash;
            public int fileSize;

            public FileAndHash(string fileName, string fileHash, int fileSize)
            {
                this.fileName = fileName.Replace('\\', '/');
                this.fileHash = fileHash;
                this.fileSize = fileSize;
            }

            public override string ToString()
            {
                return fileName + " " + fileHash + " " + fileSize;
            }

            public static FileAndHash FromString(string FileAndHashText, char Splitter = '|')
            {
                string[] FileAndHashArray = FileAndHashText.Split(Splitter);
                if (FileAndHashArray.Length == 3) return new FileAndHash(FileAndHashArray[0], FileAndHashArray[1], int.Parse(FileAndHashArray[2]));
                return new FileAndHash("", "", -1);
            }
        }

        private void GetlastVersion()
        {
            try
            {
                StreamReader sr = new StreamReader("ignoreList.txt");
                if(!sr.EndOfStream) lastVersion = sr.ReadLine();
                sr.Close();
            }
            catch
            {
                lastVersion = "0";
            }
        }
        private string[] GetIgnoreList()
        {
            socket.Send(new byte[] { 2 });

            byte[] sizeReceive = new byte[4];
            socket.Receive(sizeReceive, 0, 4, SocketFlags.None);
            uint size = 0;
            for (int i = 0; i < 4; i++)
            {
                size *= 0x100;
                size += sizeReceive[i];
            }

            FileStream fs = File.OpenWrite("ignoreList.txt");
            fs.SetLength(0);

            byte[] buffer = new byte[4096];
            int offset = 0;
            int bytesCount;
            while (offset < size)
            {
                bytesCount = socket.Receive(buffer);
                fs.Write(buffer, 0, bytesCount);
                offset += bytesCount;
            }
            fs.Close();


            List<string> IgnoreList = new List<string>();

            StreamReader sr = new StreamReader("ignoreList.txt");
            version = sr.ReadLine();
            IgnoreList.Add("ignoreList.txt");
            while (!sr.EndOfStream)
            {
                IgnoreList.Add(sr.ReadLine());
            }
            sr.Close();

            return IgnoreList.ToArray();
        }
        private FileAndHash[] GetFileListFromServer()
        {
            socket.Send(new byte[] { 0 });

            byte[] sizeReceive = new byte[4];
            socket.Receive(sizeReceive, 0, 4, SocketFlags.None);
            uint size = 0;
            for (int i = 0; i < 4; i++)
            {
                size *= 0x100;
                size += sizeReceive[i];
            }

            FileStream fs = File.OpenWrite("serverList.txt");
            fs.SetLength(0);

            byte[] buffer = new byte[4096];
            int offset = 0;
            int bytesCount;
            while (offset < size)
            {
                bytesCount = socket.Receive(buffer);
                fs.Write(buffer, 0, bytesCount);
                offset += bytesCount;
            }
            fs.Close();


            List<FileAndHash> fileAndHashes = new List<FileAndHash>();

            StreamReader sr = new StreamReader("serverList.txt");
            while (!sr.EndOfStream)
            {
                FileAndHash fileAndHash = FileAndHash.FromString(sr.ReadLine());
                if (fileAndHash.fileHash != "") fileAndHashes.Add(fileAndHash);
            }
            sr.Close();

            File.Delete("serverList.txt");
            return fileAndHashes.ToArray();
        }
        private void DeleteDirs(string path)
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
        private void DownloadFile(string fileName)
        {
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

            string fileDir = Directory.GetCurrentDirectory() + "/" + fileName;
            Directory.CreateDirectory(fileDir);
            Directory.Delete(fileDir, true);

            FileStream fs = File.OpenWrite(fileDir);
            fs.SetLength(0);

            byte[] buffer = new byte[4096];
            int offset = 0;
            int bytesCount;
            while (offset < size)
            {
                bytesCount = socket.Receive(buffer);
                fs.Write(buffer, 0, bytesCount);
                offset += bytesCount;
                progressDownload += bytesCount;
                if(lastPercent != progressDownload * 100 / sizeToDownload)
                {
                    lastPercent = progressDownload * 100 / sizeToDownload;
                    IsProgressLoadUpdate?.Invoke(lastPercent, progressDownload, sizeToDownload);
                }
            }
            fs.Close();
        }
        private void DeleteFile(string fileName)
        {
            string path = Directory.GetCurrentDirectory() + "/" + fileName;
            if (Directory.Exists(path))
                Directory.Delete(path, true);
            File.Delete(path);
        }

        private static string GetHashMd5(string path)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            FileStream fs = File.OpenRead(path);
            byte[] checkSum = md5.ComputeHash(fs);
            fs.Close();
            return BitConverter.ToString(checkSum).Replace("-", "").ToLower();
        }
        private static int GetFileCount(string path)
        {
            int FileCount = 0;

            foreach (string directory in Directory.GetDirectories(path))
            {
                FileCount += GetFileCount(directory);
            }

            foreach (string file in Directory.GetFiles(path))
            {
                FileCount++;
            }
            return FileCount;
        }
        
        
        private static bool FileISIgnore(string file, string[] ignoreList)
        {
            foreach(string ignore in ignoreList)
            {
                bool isIgnore = true;
                if (ignore.Length > file.Length) continue;

                for(int i = 0; i < ignore.Length; i++)
                {
                    if(ignore[i] != file[i])
                    {
                        isIgnore = false;
                        break;
                    }
                }
                if (isIgnore) return true;
            }
            return false;
        }
        private static FileAndHash[] GetFileListPri(string path, int pathLength, bool withHash)
        {
            FileAndHash[] FileList = new FileAndHash[GetFileCount(path)];
            int index = 0;

            foreach (string file in Directory.GetFiles(path))
            {
                if (withHash)
                {
                    FileList[index++] = new FileAndHash(file.Substring(pathLength, file.Length - pathLength), GetHashMd5(file), (int)(new FileInfo(file)).Length);
                }
                else
                {
                    FileList[index++] = new FileAndHash(file.Substring(pathLength, file.Length - pathLength), "", (int)(new FileInfo(file)).Length);
                }
            }

            foreach (string directory in Directory.GetDirectories(path))
            {
                foreach (FileAndHash file in GetFileListPri(directory, pathLength, withHash))
                {
                    FileList[index++] = file;
                }
            }
            return FileList;
        }
        private static FileAndHash[] GetFileList(string path, bool withHash = true)
        {
            return GetFileListPri(path, path.Length + 1, withHash);
        }
        private static bool Compare(FileAndHash[] filesOnServer, FileAndHash[] filesOnClient, string[] ignoreList)
        {
            List<FileManage> files = new List<FileManage>();

            foreach (FileAndHash fileS in filesOnServer)
            {
                bool Exist = false;
                foreach (FileAndHash fileC in filesOnClient)
                {
                    if (fileS.fileName == fileC.fileName && fileS.fileSize == fileC.fileSize)
                    {
                        Exist = true;
                        break;
                    }
                    else if (fileS.fileName == fileC.fileName && fileS.fileSize != fileC.fileSize)
                    {
                        if (!FileISIgnore(fileC.fileName, ignoreList)) files.Add(new FileManage(fileS.fileName, FileAction.Replace, fileS.fileSize));
                        Exist = true;
                        break;
                    }
                }
                if (!Exist) files.Add(new FileManage(fileS.fileName, FileAction.Create, fileS.fileSize));
            }

            foreach (FileAndHash fileC in filesOnClient)
            {
                bool Exist = false;
                foreach (FileAndHash fileS in filesOnServer)
                {
                    if (fileS.fileName == fileC.fileName)
                    {
                        Exist = true;
                        break;
                    }
                }
                if (!Exist) if (!FileISIgnore(fileC.fileName, ignoreList)) files.Add(new FileManage(fileC.fileName, FileAction.Delete, -1));
            }
            return files.Count > 0;
        }
        private static FileManage[] Manage(FileAndHash[] filesOnServer, FileAndHash[] filesOnClient, string[] ignoreList)
        {
            List<FileManage> files = new List<FileManage>();

            foreach (FileAndHash fileS in filesOnServer)
            {
                bool Exist = false;
                foreach (FileAndHash fileC in filesOnClient)
                {
                    if (fileS.fileName == fileC.fileName && fileS.fileHash == fileC.fileHash)
                    {
                        Exist = true;
                        break;
                    }
                    else if (fileS.fileName == fileC.fileName && fileS.fileHash != fileC.fileHash)
                    {
                        if(!FileISIgnore(fileC.fileName, ignoreList)) files.Add(new FileManage(fileS.fileName, FileAction.Replace, fileS.fileSize));
                        Exist = true;
                        break;
                    }
                }
                if (!Exist) files.Add(new FileManage(fileS.fileName, FileAction.Create, fileS.fileSize));
            }

            foreach (FileAndHash fileC in filesOnClient)
            {
                bool Exist = false;
                foreach (FileAndHash fileS in filesOnServer)
                {
                    if (fileS.fileName == fileC.fileName)
                    {
                        Exist = true;
                        break;
                    }
                }
                if (!Exist) if (!FileISIgnore(fileC.fileName, ignoreList)) files.Add(new FileManage(fileC.fileName, FileAction.Delete, -1));
            }
            return files.ToArray();
        }
    }
}
