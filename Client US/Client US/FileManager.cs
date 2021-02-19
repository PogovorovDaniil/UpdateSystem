using System.Collections.Generic;

namespace Client_US
{
    public enum FileAction {Create, Replace, Delete};
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
                        files.Add(new FileManage(fileS.fileName, FileAction.Replace, fileS.fileSize));
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
                if (!Exist) files.Add(new FileManage(fileC.fileName, FileAction.Delete, -1));
            }
            return files.ToArray();
        }
    }
}
