using StorageGlacierRetrieval.Controller;
using System;
using System.Diagnostics;

namespace StorageGlacierRetrieval
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch stop = new Stopwatch();
            stop.Start();
            new RestoreFileController();
            stop.Stop();
            Console.WriteLine("Time taken for completetion:{0}", stop.ElapsedMilliseconds);
            Console.ReadLine();
        }
    }
}
