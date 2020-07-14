using System;
using System.IO;
using System.Collections.Generic;
using CliMod;

namespace RookDB
{
    [CLIModule("RookDB", 1)]
    public static class RookDBModule
    {
        static Dictionary<string, RookDB> loadedDBs = new Dictionary<string, RookDB>();

        [CLICommand("load", "Attempts to load a file as json-formatted database.")]
        public static void Load(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Command error. Expected filename and dbname as argument."); return;
            }
            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Command error, specified file does not exist: " + args[0]); return;
            }
            if (loadedDBs.ContainsKey(args[1]))
            {
                Console.WriteLine("Command error, database already loaded with that name"); return;
            }

            RookDB db = new RookDB(File.ReadAllText(args[0]), args[0]);
            if (db.filename == RookDB.INVALID_DB_STR)
            {
                return; //logger should have captured the internal error, no need to double the output here. Just exit and be cool :)
            }
            loadedDBs.Add(args[1], db);
            Console.WriteLine("Database loaded.");
        }

        [CLICommand("unload", "Unloads an existing database from memory.")]
        public static void Unload(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Command error, expected database idenfier."); return;
            }
            if (!loadedDBs.ContainsKey(args[0]))
            {
                Console.WriteLine("Command error, no database loaded with that identifier."); return;
            }
            loadedDBs.Remove(args[0]);
            Console.WriteLine("Database unloaded.");
        }

        [CLICommand("lsdb", "Lists currently loaded databases")]
        public static void ListLoaded(string[] args)
        {
            Console.WriteLine("Currently loaded databases: " + loadedDBs.Count + " (0 modified)");
            Console.WriteLine("- ID -".PadRight(8) + "- Local Name -".PadRight(20) + "- File Location -");
            int count = 0;
            foreach (KeyValuePair<string, RookDB> pair in loadedDBs)
            {
                Console.WriteLine($"[{count}]".PadRight(8) + pair.Key.PadRight(20) + pair.Value.filename);
                count++;
            }
        }

        [CLICommand("display", "Displays record/columninfo/table from a loaded database")]
        public static void Display(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Command error, expected path to record/table/column/database"); return;
            }
            string[] pathParts= args[0].Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (!loadedDBs.ContainsKey(pathParts[0]))
            {
                Console.WriteLine("Command error, no database is loaded with that name."); return;
            }
            RookDB db = loadedDBs[pathParts[0]];

            if (pathParts.Length == 1)
            {
                //display entire database... ok, here we go!
                Console.WriteLine("--- " + pathParts[0] + " - File: " + db.filename + " - " + db.tables.Count + " tables ---");
                foreach (KeyValuePair<string, DBTable> tablePair in db.tables)
                {
                    Console.WriteLine("--- Table: " + tablePair.Value.identifier + " ---");
                    foreach (DBColumnInfo column in tablePair.Value.columns)
                    {
                        Console.WriteLine("COLUMN: " + StringHelper.LimitLength(RookDB.PrettyPrintColumn(column), 80));
                    }
                    for (int i = 0; i < tablePair.Value.records.Count; i++)
                    {
                        //only print the first 20 (maybe add a flag for this limit later)
                        if (i > 20)
                        {
                            Console.WriteLine("[... " + (tablePair.Value.records.Count - 20).ToString() + " more records ...]");
                            break;
                        }
                        Console.WriteLine("RECORD: " + StringHelper.LimitLength(RookDB.PrettyPrintRecord(tablePair.Value.records[i]), 80));
                    }
                }
            }
            else if (pathParts.Length == 2)
            {
                //display single database table
                if (!db.tables.ContainsKey(pathParts[1]))
                {
                    Console.WriteLine("Command error, no table with name: " + pathParts[1]); return;
                }
                DBTable table = db.tables[pathParts[1]];
                Console.WriteLine("--- Table: " + table.identifier + " ---");
                foreach (DBColumnInfo column in table.columns)
                {
                    Console.WriteLine("COLUMN: " + StringHelper.LimitLength(RookDB.PrettyPrintColumn(column), 80));
                }
                for (int i = 0; i < table.records.Count; i++)
                {
                    Console.WriteLine("RECORD: " + StringHelper.LimitLength(RookDB.PrettyPrintRecord(table.records[i]), 80));
                }
            }
            else if (pathParts.Length == 3)
            {
                //tricky, either column or record (assume record, but if it fails, check column jic)
                if (db.RecordExists(pathParts[1] + "/" + pathParts[2]))
                {
                    DBRecord record = db.GetRecord(pathParts[1] + "/" + pathParts[2]);
                    Console.WriteLine("--- Record: " + record.identifier + " ---");
                    for (int i = 0; i < record.values.Length; i++)
                    {
                        Console.WriteLine(record.ownerTable.columns[i].columnIdenfier.PadRight(20) + " (" 
                            + record.ownerTable.columns[i].columnType.ToString() + ") = " + RookDB.PrettyPrintFieldValue(record, i));
                    }
                }
                else if (db.ColumnExists(pathParts[1] + "/" + pathParts[2]))
                {
                    
                }
                else
                {
                    Console.WriteLine("Command error, no record or column found with name: " + pathParts[1] + "/" + pathParts[2]);
                }
            }
            else if (pathParts.Length == 4)
            {
                //this can only db/table/record/field
            }
        }
    }
}