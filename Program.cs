using System;
using CliMod;

namespace RookDB
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.Init(Logger.InitLevel.Verbose);
            bool runEditor = false;
            string editorFile = null;
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg == "-e" || arg == "--editor")
                {
                    runEditor = true;
                }
                if (arg == "-f" || arg == "--editor-file")
                {
                    if (i + 1 < args.Length)
                    {
                        editorFile = args[i + 1];
                    }
                }
            }

            if (runEditor)
            {
                Editor.EditorProgram.RunEditor(editorFile);
            }
            else
            {
                new CLInterface().Run();
            }
        }
    }
}
