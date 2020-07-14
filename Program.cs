using System;
using CliMod;

namespace RookDB
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.Init(Logger.InitLevel.Verbose);
            CLInterface cli = new CLInterface();
            cli.Run();
        }
    }
}
