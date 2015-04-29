using System;
using System.IO;

namespace INFDTA01_2.DennisBrian.Assignment
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("INFDTA Assignments");

            var path = Directory.GetFiles(Directory.GetCurrentDirectory());

            foreach (var s in path)
                Console.WriteLine(s);

            Console.ReadLine();
        }
    }
}