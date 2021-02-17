using System;
using System.IO;

namespace Client_US
{
    class Program
    {
        static void Main()
        {
            FileAndHash[] files = HashList.GetFileList(Directory.GetCurrentDirectory() + "\\dir\\");
            foreach(FileAndHash file in files) Console.WriteLine(file);
            while (true) ;
        }
    }
}
