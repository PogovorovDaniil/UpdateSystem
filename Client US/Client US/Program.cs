using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client_US
{
    class Program
    {
        static void Main()
        {
            FileAndHash[] files = HashList.GetFileList("C:\\Users\\gigst\\Documents\\GitHub\\UpdateSystem\\Server US\\dir\\");
            foreach(FileAndHash file in files) Console.WriteLine(file);
            while (true) ;
        }
    }
}
