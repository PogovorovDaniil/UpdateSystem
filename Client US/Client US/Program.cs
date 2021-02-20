using System;

namespace Client_US
{
    class Program
    {
        static void Main()
        {
            UpdateServer updateServer = new UpdateServer("193.178.169.223", 9090);
            updateServer.IsProgressLoadUpdate += UpdateServer_IsProgressLoadUpdate;
            int sizeFiles;
            if (updateServer.TryConnect(out sizeFiles))
            {
                Console.WriteLine("Подключение к серверу успешно!");
                Console.WriteLine("Требуется скачать {0} byte", sizeFiles);

                if (updateServer.UpdateFileSystem())
                {
                    Console.WriteLine("Обновление успешно!");
                }
                else
                {
                    Console.WriteLine("Обновление провалено.");
                }
            }
            else
            {
                Console.WriteLine("Подключение к серверу провалено.");
            }
            while (true) ;
        }

        private static void UpdateServer_IsProgressLoadUpdate(int percent, int progress, int size)
        {
            Console.WriteLine("{0}/{1} - {2}%", progress, size, percent);
        }
    }
}
