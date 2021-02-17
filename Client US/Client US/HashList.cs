﻿using System;
using System.IO;
using System.Security.Cryptography;

namespace Client_US
{
    public struct FileAndHash
    {
        public string FileName;
        public string FileHash;

        public FileAndHash(string FileName, string FileHash)
        {
            this.FileName = FileName;
            this.FileHash = FileHash;
        }

        public override string ToString()
        {
            return FileName + " " + FileHash;
        }

        public FileAndHash FromString(string FileAndHashText, char Splitter = '|')
        {
            string[] FileAndHashArray = FileAndHashText.Split(Splitter);
            if (FileAndHashArray.Length > 1) return new FileAndHash(FileAndHashArray[0], FileAndHashArray[1]);
            return new FileAndHash("", "");
        }
    }
    public static class HashList
    {
        private static MD5 md5 = new MD5CryptoServiceProvider();

        public static string GetHashMd5(string path)
        {
            byte[] checkSum = md5.ComputeHash(File.OpenRead(path));
            return BitConverter.ToString(checkSum).Replace("-", "").ToLower();
        }
        public static int GetFileCount(string path)
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
        private static FileAndHash[] GetFileListPri(string path, int pathLength)
        {
            FileAndHash[] FileList = new FileAndHash[GetFileCount(path)];
            int index = 0;

            foreach (string file in Directory.GetFiles(path))
            {
                FileList[index++] = new FileAndHash(file.Substring(pathLength, file.Length - pathLength), GetHashMd5(file));
            }

            foreach (string directory in Directory.GetDirectories(path))
            {
                foreach (FileAndHash file in GetFileListPri(directory, pathLength))
                {
                    FileList[index++] = file;
                }
            }
            return FileList;
        }
        public static FileAndHash[] GetFileList(string path)
        {
            return GetFileListPri(path, path.Length);
        }
    }
}
