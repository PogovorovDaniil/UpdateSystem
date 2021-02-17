using System;
using System.Collections.Generic;

namespace Client_US
{
    public enum FileAction {Create, Replace, Delete};
    public struct FileManage
    {
        public string fileName;
        public FileAction fileAction;

        public FileManage(string fileName, FileAction fileAction)
        {
            this.fileName = fileName;
            this.fileAction = fileAction;
        }

        public override string ToString()
        {
            return fileName + " " + fileAction;
        }
    }
    public static class FileManager
    {
        public static FileManage[] Manage(FileAndHash[] filesOnServer, FileAndHash[] filesOnClient)
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
                        files.Add(new FileManage(fileS.fileName, FileAction.Replace));
                        Exist = true;
                        break;
                    }
                }
                if (!Exist) files.Add(new FileManage(fileS.fileName, FileAction.Create));
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
                if (!Exist) files.Add(new FileManage(fileC.fileName, FileAction.Delete));
            }
            return files.ToArray();
        }
    }
}
