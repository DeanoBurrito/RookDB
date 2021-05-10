using System;

namespace RookDB
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello world, from v2 development branch.");
            ReadonlyDB rodb = ReadonlyDB.LoadFile("_Data/ExampleDB.cdb");
        }
    }
}
