using System;
using System.Diagnostics;

namespace GZipTest
{
    class Program
    {
        
        static int Main(string[] args)
        {
            var timer = Stopwatch.StartNew();
            if (args.Length != 3)
            {
                Console.WriteLine("Не правильное количество аргументов");
                return 1;
            }
            if (args[0].ToLower() == "compress")
            {
                try
                {
                    CompressFile compressFile = new CompressFile(args[1], args[2]);
                    compressFile.Processing();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Программа закончена с ОШИБКОЙ!");
                    //Console.ReadKey();
                    return 1;
                }
            }
            else if (args[0].ToLower() == "decompress")
            {
                try
                {
                    DecompressFile decompressFile = new DecompressFile(args[1], args[2]);
                    decompressFile.Processing();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Программа закончена с ОШИБКОЙ!");
                    //Console.ReadKey();
                    return 1;
                }
            }
            else
            {
                Console.WriteLine("Не задан режим работы программы: compress/decompress");
                return 1;
            }
            timer.Stop();
            TimeSpan ts = timer.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}",
                ts.Hours, ts.Minutes, ts.Seconds);
            Console.WriteLine("Программа закончена ({0})", elapsedTime);
            //Console.ReadKey();
            return 0;
        }
    }
}
