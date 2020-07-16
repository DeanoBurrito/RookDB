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

        [CLICommand("mvdb", "Renames a loaded database (not filename, only in-memory name)")]
        public static void RenameLoaded(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Command error, need source and destination names."); return;
            }
            if (!loadedDBs.ContainsKey(args[0]))
            {
                Console.WriteLine("Command error, source database does not exist with name=" + args[0]); return;
            }
            if (loadedDBs.ContainsKey(args[1]))
            {
                Console.WriteLine("Command error, database already exists with destination name=" + args[1]); return;
            }

            RookDB db = loadedDBs[args[0]];
            loadedDBs.Remove(args[0]);
            loadedDBs.Add(args[1], db);
            Console.WriteLine($"Renamed database {args[0]} > {args[1]}");
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
            const int LINE_WIDTH_LIMIT = 84;

            if (pathParts.Length == 1)
            {
                //display entire database... ok, here we go!
                Console.WriteLine("--- " + pathParts[0] + ", File: " + db.filename + ", " + db.tables.Count + " tables, " + db.embeddedSchemas.Count +  " list schemas ---");
                foreach(KeyValuePair<string, List<DBColumnInfo>> cols in db.embeddedSchemas)
                {
                    Console.WriteLine("--- Embedded List Schema: " + cols.Key + " ---");
                    foreach (DBColumnInfo column in cols.Value)
                    {
                        Console.WriteLine("COLUMN: " + StringHelper.LimitLength(RookDB.PrettyPrintColumn(column), LINE_WIDTH_LIMIT));
                    }
                }
                foreach (KeyValuePair<string, DBTable> tablePair in db.tables)
                {
                    Console.WriteLine("--- Table: " + tablePair.Value.identifier + " ---");
                    foreach (DBColumnInfo column in tablePair.Value.columns)
                    {
                        Console.WriteLine("COLUMN: " + StringHelper.LimitLength(RookDB.PrettyPrintColumn(column), LINE_WIDTH_LIMIT));
                    }
                    int idx = 0;
                    foreach (DBRecord record in tablePair.Value.records.Values)
                    {
                        if (idx > 20)
                        {
                            Console.WriteLine("[... " + (tablePair.Value.records.Count - 20).ToString() + " more records ...]");
                            break;
                        }
                        idx++;
                        Console.WriteLine("RECORD: " + StringHelper.LimitLength(RookDB.PrettyPrintRecord(record), LINE_WIDTH_LIMIT));
                    }
                }
            }
            else if (pathParts.Length == 2)
            {
                //display single database table
                if (!db.tables.ContainsKey(pathParts[1]))
                {
                    if (!db.embeddedSchemas.ContainsKey(pathParts[1]))
                    {
                        Console.WriteLine("Command error, no table with name: " + pathParts[1]); return;
                    }
                    //its a schema, just display that
                    List<DBColumnInfo> columns = db.embeddedSchemas[pathParts[1]];
                    Console.WriteLine("--- Embedded List Schema: " + pathParts[1] + " ---");
                    foreach (DBColumnInfo column in columns)
                    {
                        Console.WriteLine("COLUMN: " + StringHelper.LimitLength(RookDB.PrettyPrintColumn(column), LINE_WIDTH_LIMIT));
                    }
                    return;
                }
                DBTable table = db.tables[pathParts[1]];
                Console.WriteLine("--- Table: " + table.identifier + " ---");
                foreach (DBColumnInfo column in table.columns)
                {
                    Console.WriteLine("COLUMN: " + StringHelper.LimitLength(RookDB.PrettyPrintColumn(column), LINE_WIDTH_LIMIT));
                }
                foreach (DBRecord record in table.records.Values)
                {
                    Console.WriteLine("RECORD: " + StringHelper.LimitLength(RookDB.PrettyPrintRecord(record), LINE_WIDTH_LIMIT));
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
                        Console.WriteLine(record.ownerTable.columns[i].columnIdenfier.PadRight(20) + (" (" 
                            + record.ownerTable.columns[i].columnType.ToString() + ")").PadRight(20) + " = " + RookDB.PrettyPrintFieldValue(record, i));
                    }
                }
                else if (db.ColumnExists(pathParts[1] + "/" + pathParts[2]))
                {
                    DBColumnInfo column = db.GetColumnInfo(pathParts[1] + "/" + pathParts[2]);
                    Console.WriteLine("--- Column: " + column.columnIdenfier + " ---");
                    Console.WriteLine("Type = " + column.columnType.ToString());
                    if (column.columnMeta != null)
                        Console.WriteLine("Metadata = " + StringHelper.LimitLength(StringHelper.SquishArray(column.columnMeta), LINE_WIDTH_LIMIT));
                    else
                        Console.WriteLine("Metadata = (null)");
                }
                else
                {
                    Console.WriteLine("Command error, no record or column found with name: " + pathParts[1] + "/" + pathParts[2]);
                }
            }
            else if (pathParts.Length == 4)
            {
                //this can only db/table/record/field
                if (!db.RecordExists(pathParts[1] + "/" + pathParts[2]))
                {
                    Console.WriteLine("Command error, no record found with that name."); return;
                }
                if (!db.ColumnExists(pathParts[1] + "/" + pathParts[3]))
                {
                    Console.WriteLine("Command error, no field/column found with that name."); return;
                }

                Console.WriteLine("--- Field: " + pathParts[1] + "/" + pathParts[2] + "/" + pathParts[3] + " ---");
                List<DBColumnInfo> columnInfos = db.GetColumns(pathParts[1]);
                int index = 0;
                for (; index < columnInfos.Count; index++)
                {
                    if (columnInfos[index].columnIdenfier == pathParts[3])
                    { break; }
                }
                //no need to verify column exists, we checked above.
                Console.WriteLine(RookDB.PrettyPrintField(db.GetRecord(pathParts[1] + "/" + pathParts[2]), index));
            }
        }

        [CLICommand("editor", "Launches the RookDB editor.")]
        public static void LaunchEditor(string[] args)
        {
            Editor.EditorProgram.RunEditor();
        }
    }
}